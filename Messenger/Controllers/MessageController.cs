using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using Messenger.Services;

namespace Messenger.Controllers;

/// <summary>
/// Контроллирует сообщения
/// </summary>
[Route("api/messenger")]
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

        return Ok(await _context.Messages.Where(m => m.UserTo == jwt!.Id || m.UserFrom == jwt.Id).ToListAsync());
    }

    /// <summary>
    /// Получить историю сообщений для двух конкретных пользователей. Первый пользователь по JWT.
    /// </summary>
    /// <param name="userID">Второй пользователь</param>
    /// <returns>Массив сообщений между двумя пользователями</returns>
    /// <response code="401">Ошибка авторизации</response>
    /// <response code="200">История сообщений</response>
    /// <response code="404">Пользователь с заданным userId не был найден</response>
    [HttpGet("history/{userID}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(List<Message>), 200)]
    public async Task<IActionResult> GetHistoryWith(Guid userID)
    {
        if (!await _context.AuthUsers.AnyAsync(u => u.Id == userID))
            return NotFound($"User with id: {userID} was not found");

        var jwt = await JwtTokenStatics.GetUserInfoAsync(HttpContext);

        return Ok(await _context.Messages.Where(m => m.UserTo == jwt!.Id && m.UserFrom == userID ||
                                                     m.UserTo == userID && m.UserFrom == jwt.Id)
            .OrderBy(t => t.DateSent)
            .ToListAsync());
    }


    /// <summary>
    /// Endpoint для подключения к мессенджеру по websocket.
    /// </summary>
    /// <returns></returns>
    /// <response code="401">Попытка подключения не по протоколу websocket</response>
    /// <response code="404">Указанный пользователь не найден</response>
    /// <param name="accessToken">JWT-токен доступа</param>
    [HttpGet("connect")]
    public async Task Connect(string accessToken)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("This connection is a websocket connection"));
            return;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        TrackUser? jwt;
        try
        {
            tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = JwtTokenStatics.Issuer,
                ValidAudience = JwtTokenStatics.Audience,
                ValidateAudience = true,

                IssuerSigningKey = JwtTokenStatics.SecurityKey,
                ValidateIssuerSigningKey = true
            }, out var token);

            jwt = JwtTokenStatics.DecipherJWT((JwtSecurityToken) token);
        }
        catch (Exception) { jwt = null; }

        if (jwt is null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await HttpContext.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("Authorization error"));
            return;
        }

        var user = await _context.AuthUsers.FirstOrDefaultAsync(x => x.Id == jwt.Id);
        if (user == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes($"User was not found"));
            return;
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        await _messenger.HandleMessages(new MessengerClient(webSocket, user), _context);
    }
}
