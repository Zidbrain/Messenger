using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Messenger.Controllers;

/// <summary>
/// Информация о пользователе для авторизации
/// </summary>
public record class UserAuthInfo
{
    /// <summary>
    /// Имя пользователя
    /// </summary>
    [Required]
    public string Username { get; set; }

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    [Required]
    public string Password { get; set; }

    public UserAuthInfo(string username, string password) =>
        (Username, Password) = (username, password);
}

/// <summary>
/// Контроллер информации о пользователях
/// </summary>
[Route("[controller]")]
[ApiController]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly MessengerContext _context;

    public UserController(MessengerContext context) =>
        _context = context;

    /// <summary>
    /// Получения списка всех пользователей. Требует аунтетификации.
    /// </summary>
    /// <returns>Список пользователей с логинами и id</returns>
    /// <response code="401">Ошибка аунтетификации.</response>
    [HttpGet("all")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(List<ClientUserInfo>), 200)]
    public async Task<IActionResult> GetUsersList() =>
        Ok((await _context.AuthUsers.ToListAsync()).Select(x => new ClientUserInfo(x)));


    /// <summary>
    /// Получение информации о конкретном пользователе. Требует аунтетификации.
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <returns>Информация о пользователе</returns>
    /// <response code="404">Пользователь не найден</response>
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ClientUserInfo), 200)]
    public async Task<IActionResult> GetUserInfo([FromQuery] Guid id)
    {
        var user = await _context.AuthUsers.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
            return NotFound();

        return Ok(new ClientUserInfo(user));
    }

    public record class UserRegisterInfo(string Username, string Password, string Nickname);

    /// <summary>
    /// Регистрация пользователя
    /// </summary>
    /// <param name="user">Информация о пользователе, которого необходимо зарегистрировать</param>
    /// <returns>Зарегистрированный пользователь</returns>
    /// <response code="409">Пользователь с данным именем уже существует</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthUser), 201)]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegisterInfo user)
    {
        if (await _context.AuthUsers.AnyAsync(x => x.Username == user.Username))
            return Conflict(new { reason = "User with given username already exists" });

        var entry = await _context.AuthUsers.AddAsync(new AuthUser(user.Username, user.Password) { Nickname = user.Nickname });
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUserInfo), entry.Entity.Id, entry.Entity);
    }

    public record class UserPatchInfo(string Nickname, string PhoneNumber, string Status);

    /// <summary>
    /// Изменить информацию пользователя. Пользователь определяется по jwt-токену.
    /// </summary>
    /// <param name="info">Информация для изменения</param>
    /// <returns>Изменённый пользователь</returns>
    /// <response code="404">Пользователь с данным ID не найден</response>
    [HttpPatch]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ClientUserInfo), 200)]
    public async Task<IActionResult> PatchUser([FromBody] UserPatchInfo info)
    {
        var jwt = await JwtTokenStatics.GetUserInfoAsync(HttpContext);

        var user = await _context.AuthUsers.FirstOrDefaultAsync(x => x.Id == jwt.Id);

        if (user == null)
            return NotFound();

        user.PhoneNumber = info.PhoneNumber;
        user.Status = info.Status;
        user.Nickname = info.Nickname;

        await _context.SaveChangesAsync();

        return Ok(new ClientUserInfo(user));
    }
}
