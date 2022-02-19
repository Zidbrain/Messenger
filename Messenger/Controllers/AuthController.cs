using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Messenger.Controllers;

public class TrackUser
{
    public Guid Id { get; set; }
    public byte[] Password { get; set; }

    public TrackUser(Guid id, byte[] password) => (Id, Password) = (id, password);

    public List<Claim> GetClaims() => new()
    {
        new Claim(ClaimsIdentity.DefaultNameClaimType, Id.ToString()),
        new Claim(ClaimsIdentity.DefaultRoleClaimType, Encoding.UTF8.GetString(Password))
    };
}


[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly MessengerContext _context;

    public AuthController(MessengerContext context)
    {
       _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Get([FromBody] UserInfoBody user)
    {
        var identity = await GetIdentity(user.Username, user.Password);
        if (identity == null)
            return BadRequest(new { errorText = "Invalid username or password" });

        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: JwtTokenStatics.Issuer,
            audience: JwtTokenStatics.Audience,
            notBefore: now,
            expires: now.AddMinutes(60),
            claims: identity.Claims,
            signingCredentials: new SigningCredentials(JwtTokenStatics.SecurityKey, SecurityAlgorithms.HmacSha256)
            );
        var encoded = new JwtSecurityTokenHandler().WriteToken(jwt);

        return Ok(new
        {
            access_token = encoded
        });
    }

    private async Task<ClaimsIdentity?> GetIdentity(string username, string password)
    {
        var hash = AuthUser.GetHash(password);
        var dbuser = await _context.AuthUsers.FirstOrDefaultAsync(x => x.Username == username && x.Password.SequenceEqual(hash));
        if (dbuser == null)
            return null;

        var user = new TrackUser(dbuser.Id, hash); 
        return new ClaimsIdentity(user.GetClaims(), "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
    }
}
