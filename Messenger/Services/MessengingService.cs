using System.Net.WebSockets;
using System.Collections.Concurrent;

namespace Messenger.Services;

public class MessengingService
{
    private readonly ConcurrentDictionary<Guid, MessengerClient> _clients = new();

    public async Task HandleMessages(MessengerClient client, MessengerContext context)
    {
        (var user, var webSocket) = (client.UserInfo, client.WebSocket);

        _clients.AddOrUpdate(user.Id, new MessengerClient(webSocket, user), (guid, client) => throw new Exception());

        var source = new CancellationTokenSource();
        var token = source.Token;

        try
        {
            foreach (var message in user.MessageUserToNavigations.Where(m => !m.IsDelivered))
            {
                await _clients[user.Id].SendToClientAsync(message, token);
                message.IsDelivered = true;
            }

            await context.SaveChangesAsync();

            while (true)
            {
                var message = await _clients[user.Id].RecieveFromClientAsync(token);

                if (message is null)
                {
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, token);
                    _clients.TryRemove(user.Id, out _);
                    break;
                }
                else
                {
                    context.Messages.Add(message);
                    
                    if (_clients.ContainsKey(message.UserTo))
                    {
                        try
                        {
                            await _clients[message.UserTo].SendToClientAsync(message, CancellationToken.None);
                            message.IsDelivered = true;
                        }
                        finally
                        {
                            await context.SaveChangesAsync();
                        }
                    }
                    else
                        await context.SaveChangesAsync();
                }
            }
        }
        finally
        {
            source.Cancel();
            _clients.TryRemove(user.Id, out _);

            if (webSocket.State is not WebSocketState.Closed)
                await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, null, CancellationToken.None);
        }
    }
}
