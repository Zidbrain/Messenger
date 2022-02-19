using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using Messenger.Services;

namespace Messenger.Controllers;
[Route("messenger")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly MessengerContext _context;
    private readonly MessengingService _messenger;

    public MessageController(MessengerContext context, MessengingService messenger)
    {
        _context = context;
        _messenger = messenger;
    }

    [HttpGet("history")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetHistory()
    {
        var jwt = JwtTokenStatics.GetUserInfo((await HttpContext.GetTokenAsync("access_token"))!);

        return Ok(await _context.Messages.Where(m => m.UserTo == jwt.Id).ToListAsync());
    }

    [HttpGet("connect")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task Connect(string username)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("This connection is a websocket connection"));
            return;
        }

        var user = await _context.AuthUsers.FirstOrDefaultAsync(x => x.Username == username);
        if (user == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes($"User {username} not found"));
            return;
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        await _messenger.HandleMessages(new MessengerClient(webSocket, user), _context);
    }
}
