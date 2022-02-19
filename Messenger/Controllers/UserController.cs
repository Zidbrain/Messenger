using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Messenger.Controllers;

public record class UserInfoBody(string Username, string Password);

[Route("[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly MessengerContext _context;

    public UserController(MessengerContext context) =>
        _context = context;

    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetUsersList() =>
        Ok(await _context.AuthUsers.ToListAsync());

    [HttpGet("{username}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetUserInfo(string username)
    {
        var user = await _context.AuthUsers.FirstOrDefaultAsync(x => x.Username == username);
        if (user == null)
            return NotFound();

        return Ok();
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserInfoBody user)
    {
        if (await _context.AuthUsers.AnyAsync(x => x.Username == user.Username))
            return Conflict(new { reason = "User with given username already exists" });

        var entry = await _context.AuthUsers.AddAsync(new AuthUser(user.Username, user.Password));
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUserInfo), new { username = user.Username }, entry.Entity);
    }
}
