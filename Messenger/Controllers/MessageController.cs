using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using Messenger.Services;

namespace Messenger.Controllers;

/// <summary>
/// Контроллирует сообщения
/// </summary>
[Route("messenger")]
[ApiController]
[Produces("application/json")]
public class MessageController : ControllerBase
{
    private readonly MessengerContext _context;
    private readonly MessengingService _messenger;

    /// <summary>
    /// Создание контроллера.
    /// </summary>
    /// <param name="context">Контекст бд</param>
    /// <param name="messenger">Сервис мессенджера</param>
    public MessageController(MessengerContext context, MessengingService messenger)
    {
        _context = context;
        _messenger = messenger;
    }

    /// <summary>
    /// Получение истории сообщений для конкретного пользователя. Пользователь определяется по JWT токену.
    /// </summary>
    /// <returns>Массив сообщений от или до данного пользователя</returns>
    /// <response code="401">Ошибка авторизации</response>
    /// <response code="200">История сообщений</response>
    [HttpGet("history")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(List<Message>), 200)]
    public async Task<IActionResult> GetHistory()
    {
        var jwt = await JwtTokenStatics.GetUserInfoAsync(HttpContext);

        return Ok(await _context.Messages.Where(m => m.UserTo == jwt.Id || m.UserFrom == jwt.Id).ToListAsync());
    }


    /// <summary>
    /// Endpoint для подключения к мессенджеру по websocket. Требует авторизации.
    /// </summary>
    /// <returns></returns>
    /// <response code="401">Попытка подключения не по протоколу websocket</response>
    /// <response code="404">Указанный пользователь не найден</response>
    [HttpGet("connect")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task Connect()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("This connection is a websocket connection"));
            return;
        }

        var jwt = await JwtTokenStatics.GetUserInfoAsync(HttpContext);

        var user = await _context.AuthUsers.FirstAsync(x => x.Id == jwt.Id);
        //if (user == null)
        //{
        //    HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        //    await HttpContext.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes($"User {username} not found"));
        //    return;
        //}

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        await _messenger.HandleMessages(new MessengerClient(webSocket, user), _context);
    }
}
