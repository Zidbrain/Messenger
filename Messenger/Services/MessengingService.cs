using System.Net.WebSockets;
using System.Collections.Concurrent;

namespace Messenger.Services;

public class MessengingService
{
    private readonly ConcurrentDictionary<Guid, MessengerClient> _clients = new();

    private static async Task DeliverAllMessages(MessengerClient client, MessengerContext context, CancellationToken token)
    {
        var messages = client.UserInfo.MessageUserToNavigations.
            Where(m => !m.IsDelivered)
            .OrderBy(m => m.DateSent)
            .ToArray();

        await client.SendMessageArrayAsync(messages, token);

        foreach (var message in messages)
        {
            message.IsDelivered = true;
        }

        await context.SaveChangesAsync(token);
    }

    public async Task HandleMessages(MessengerClient client, MessengerContext context)
    {
        (var user, var webSocket) = (client.UserInfo, client.WebSocket);

        _clients.AddOrUpdate(user.Id, new MessengerClient(webSocket, user), (guid, client) => throw new Exception());

        var source = new CancellationTokenSource();
        var token = source.Token;

        try
        {
            await DeliverAllMessages(client, context, token);

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

            if (webSocket.State is not WebSocketState.Closed and not WebSocketState.Aborted)
                await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, null, CancellationToken.None);
        }
    }
}
