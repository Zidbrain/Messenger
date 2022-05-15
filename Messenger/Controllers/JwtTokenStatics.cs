using Messenger.Controllers;
using System.Security.Claims;

namespace Messenger;

public static class JwtTokenStatics
{
    public const string Issuer = "https://localhost:7230/";
    public const string Audience = "https://localhost/";

    public static TokenValidationParameters TokenValidationParameters { get; } = new()
    {
        ValidateIssuer = true,
        ValidIssuer = Issuer,
        ValidAudience = Audience,
        ValidateAudience = true,

        IssuerSigningKey = SecurityKey,
        ValidateIssuerSigningKey = true
    };

    public static SymmetricSecurityKey SecurityKey { get; } =
            new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(64));

    public static async Task<TrackUser?> GetUserInfoAsync(HttpContext context)
    {
        var jwt = await context.GetTokenAsync("access_token");
        var token = new JwtSecurityToken(jwt);

        return DecipherJWT(token);
    }

    public static TrackUser? DecipherJWT(JwtSecurityToken token)
    {
        try
        {
            return new TrackUser(
                Guid.Parse(token.Claims.First(x => x.Type == "name").Value),
                Encoding.UTF8.GetBytes(token.Claims.First(x => x.Type == "role").Value));
        }
        catch (Exception)
        {
            return null;
        }
    }
}
