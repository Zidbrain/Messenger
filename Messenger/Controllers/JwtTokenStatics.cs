using Messenger.Controllers;
using System.Security.Claims;

namespace Messenger;

public static class JwtTokenStatics
{
    public const string Issuer = "https://localhost:7230/";
    public const string Audience = "https://localhost/";

    public static SymmetricSecurityKey SecurityKey { get; } = 
        new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(64));

    public static TrackUser GetUserInfo(string jwt)
    {
        var token = new JwtSecurityToken(jwt);
        return new TrackUser(
            Guid.Parse(token.Claims.First(x => x.Type == ClaimsIdentity.DefaultNameClaimType).Value),
            Encoding.UTF8.GetBytes(token.Claims.First(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value));
    }
}
