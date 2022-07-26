using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Messenger.Controllers;

/// <summary>
/// Модель пользователя для хранения внутри сервера
/// </summary>
public class TrackUser
{
    /// <summary>
    /// Индетификатор пользователя
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Хэшированный пароль пользователя
    /// </summary>
    public byte[] Password { get; set; }

    /// <summary>
    /// Стандартный конструктор.
    /// </summary>
    /// <param name="id">Индетификатор пользователя</param>
    /// <param name="password">Хэшированный пароль пользователя</param>

    public TrackUser(Guid id, byte[] password) => (Id, Password) = (id, password);

    public List<Claim> GetClaims() => new()
    {
        new Claim(ClaimsIdentity.DefaultNameClaimType, Id.ToString()),
        new Claim(ClaimsIdentity.DefaultRoleClaimType, Encoding.UTF8.GetString(Password))
    };
}


/// <summary>
/// Контроллер авторизации пользователей.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly MessengerContext _context;

    /// <summary>
    /// Стандартный конструктор
    /// </summary>
    /// <param name="context"></param>
    public AuthController(MessengerContext context)
    {
       _context = context;
    }

    private record class AccessTokenJSON(string AccessToken);

    /// <summary>
    /// Получить токен аунтетификации пользователя.
    /// </summary>
    /// <param name="user">Данные пользователя</param>
    /// <returns>Токен аунтетификации</returns>
    /// <response code="400">Неправильное имя пользователя или пароль</response>
    /// <remarks>
    /// Пример запроса:
    ///     POST /Auth
    ///     {
    ///         "username": "user",
    ///         "password": "password"
    ///     }
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(AccessTokenJSON), 200)]
    public async Task<IActionResult> Get([FromBody] UserAuthInfo user)
    {
        var identity = await GetIdentity(user.Username, user.Password);
        if (identity == null)
            return BadRequest(new { errorText = "Invalid username or password" });

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            Issuer = JwtTokenStatics.Issuer,
            Audience = JwtTokenStatics.Audience,
            NotBefore = DateTime.Now,
            SigningCredentials = new SigningCredentials(JwtTokenStatics.SecurityKey, SecurityAlgorithms.HmacSha256)
        };
        var encoded =  tokenHandler.CreateEncodedJwt(tokenDescriptor);

        //var now = DateTime.UtcNow;
        //var jwt = new JwtSecurityToken(
        //    issuer: JwtTokenStatics.Issuer,
        //    audience: JwtTokenStatics.Audience,
        //    notBefore: now,
        //    claims: identity.Claims,
        //    signingCredentials: new SigningCredentials(JwtTokenStatics.SecurityKey, SecurityAlgorithms.HmacSha256)
        //    );
        //var encoded = new JwtSecurityTokenHandler().WriteToken(jwt);

        return Ok(new AccessTokenJSON(encoded));
    }

    private async Task<ClaimsIdentity?> GetIdentity(string username, string password)
    {
        var dbuser = await _context.AuthUsers.FirstOrDefaultAsync(x => x.Username == username);
        if (dbuser is null)
            return null;

        if (!dbuser.CheckHash(password, out var hash))
            return null;

        var user = new TrackUser(dbuser.Id, hash); 
        return new ClaimsIdentity(user.GetClaims(), "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
    }
}
