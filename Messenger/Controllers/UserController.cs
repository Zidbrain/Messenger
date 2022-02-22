using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Messenger.Controllers;

/// <summary>
/// Информация о пользователе для авторизации
/// </summary>
/// <param name="Username">Имя пользователя</param>
/// <param name="Password">Пароль пользователя</param>
public record class UserAuthInfo
{
    [Required]
    public string Username { get; set; }

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
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(List<AuthUser>), 200)]
    public async Task<IActionResult> GetUsersList() =>
        Ok(await _context.AuthUsers.ToListAsync());


    /// <summary>
    /// Получение информации о конкретном пользователе. Требует аунтетификации.
    /// </summary>
    /// <param name="username">Имя пользователя</param>
    /// <returns>Информация о пользователе</returns>
    /// <response code="404">Пользователь не найден</response>
    [HttpGet("{username}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(AuthUser), 200)]
    public async Task<IActionResult> GetUserInfo(string username)
    {
        var user = await _context.AuthUsers.FirstOrDefaultAsync(x => x.Username == username);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Регистрация пользователя
    /// </summary>
    /// <param name="user">Информация о пользователе, которого необходимо зарегистрировать</param>
    /// <returns>Зарегистрированный пользователь</returns>
    /// <response code="409">Пользователь с данным именем уже существует</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthUser), 201)]
    public async Task<IActionResult> RegisterUser([FromBody] UserAuthInfo user)
    {
        if (await _context.AuthUsers.AnyAsync(x => x.Username == user.Username))
            return Conflict(new { reason = "User with given username already exists" });

        var entry = await _context.AuthUsers.AddAsync(new AuthUser(user.Username, user.Password));
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUserInfo), new { username = user.Username }, entry.Entity);
    }
}
