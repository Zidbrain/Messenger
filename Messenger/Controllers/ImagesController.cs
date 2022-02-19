using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Messenger.Controllers;
[Route("[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ImagesController : ControllerBase
{
    private readonly ILogger<ImagesController> _logger;
    private readonly MessengerContext _context;

    public ImagesController(ILogger<ImagesController> logger, MessengerContext dbContext) =>
        (_logger, _context) = (logger, dbContext);

    [HttpPost]
    public async Task<IActionResult> PostImage([FromForm(Name = "image")] IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".png" or ".jpg"))
            return BadRequest("The file must be an image file");

        var jwt = JwtTokenStatics.GetUserInfo((await HttpContext.GetTokenAsync("access_token"))!);

        AuthUser user;
        try
        {
            var src = $"/Images/{jwt.Id}{extension}";
            using var fileStream = System.IO.File.Create("." + src);
            await file.CopyToAsync(fileStream, CancellationToken.None);

            user = await _context.AuthUsers.FirstAsync(x => x.Id.CompareTo(jwt.Id) == 0);
            user.ImageSrc = src;

            _context.AuthUsers.Update(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error uploading file from {0}:{1}", jwt.Id, ex);
            return Conflict("Error uploading file");
        }

        return CreatedAtAction(nameof(GetImage), new { username = user.Username}, null);
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetImage(string username)
    {
        var user = await _context.AuthUsers.FirstOrDefaultAsync(x => x.Username == username);
        if (user == null)
            return NotFound();

        try
        {
            var fileStream = System.IO.File.OpenRead("." + user.ImageSrc);
            return File(fileStream, Path.GetExtension(fileStream.Name) == ".png" ? "image/png" : "image/jpg");
        }
        catch (FileNotFoundException)
        {
            return BadRequest("No such file");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error uploading file from {0}:{1}", username, ex);
            return Conflict("Error downloading file");
        }
    }
}
