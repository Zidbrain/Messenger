﻿using System.Net.WebSockets;
using Messenger.Models;
using System.Text.Json;

namespace Messenger.Services;

public class MessengerClient
{
    private readonly WebSocket _webSocket;
    private readonly AuthUser _userInfo;

    private byte[] _buffer = Array.Empty<byte>();

    public MessengerClient(WebSocket webSocket, AuthUser user)
    {
        _webSocket = webSocket;
        _userInfo = user;
    }

    public AuthUser UserInfo => _userInfo;

    public WebSocket WebSocket => _webSocket;

    public async Task<Message?> RecieveFromClientAsync(CancellationToken cancellationToken)
    {
        var count = 0;

        WebSocketReceiveResult? result;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            var buf = new byte[_buffer.Length + 1024 * 4];
            Array.Copy(_buffer, buf, _buffer.Length);
            _buffer = buf;

            result = await _webSocket.ReceiveAsync(_buffer, cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
                return null;

            count += result!.Count;

        } while (!result!.EndOfMessage);

        using var stream = new MemoryStream(_buffer, 0, count);

        var message = await JsonSerializer.DeserializeAsync<Message>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web), cancellationToken);
        message!.Id = Guid.NewGuid();
        message!.UserFrom = _userInfo.Id;

        return message;
    }

    public async Task SendToClientAsync(Message message, CancellationToken cancellationToken)
    {
        var prev = message.UserTo;
        message.UserTo = default;

        using var stream = new MemoryStream();

        await JsonSerializer.SerializeAsync(stream, message, new JsonSerializerOptions(JsonSerializerDefaults.Web), cancellationToken);

        await _webSocket.SendAsync(stream.GetBuffer(), WebSocketMessageType.Text, true, cancellationToken);

        message.UserTo = prev;
    }
}