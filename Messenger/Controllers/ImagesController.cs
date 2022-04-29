using Messenger.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio;

namespace Messenger.Controllers;

/// <summary>
/// Контроллер для управления изображениями (аватарками) ползьвателей. Все действия требуют аунтетификации.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ImagesController : ControllerBase
{
    private readonly ILogger<ImagesController> _logger;
    private readonly MessengerContext _context;

    private readonly IFileService _fileService;

    public ImagesController(ILogger<ImagesController> logger, MessengerContext dbContext, MinioFileServiceFactory minioFactory)
    {
        (_logger, _context) = (logger, dbContext);

        _fileService = minioFactory.CreateInstance(new MinioClient()
                .WithCredentials("imagesadmin", "imagesadmin")
                .WithEndpoint("host.docker.internal:9000")
                .Build());
    }

    /// <summary>
    /// Загрузить аватарку пользователя. Пользователь определяется JWT токеном при аунтетификации.
    /// Файл изображения должен быть формата .png или .jpg и загружен с помощью form-data
    /// </summary>
    /// <param name="file">Поле в форме с именем image</param>
    /// <returns>Endpoint для получения загруженного изображения</returns>
    /// <response code="400">Загруженный файл не является изображением</response>
    /// <response code="409">Ошибка при загрузке изображения</response>
    [HttpPost]
    public async Task<IActionResult> PostImage(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".png" or ".jpg"))
            return BadRequest("The file must be an image file");

        var jwt = await JwtTokenStatics.GetUserInfoAsync(HttpContext);

        AuthUser user;
        try
        {
            var src = $"{jwt.Id}{extension}";

            using var stream = file.OpenReadStream();
            await _fileService.SaveFile("userimages", src, stream);

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

        return CreatedAtAction(nameof(GetImage), user.Id, null);
    }

    /// <summary>
    /// Получить аватарку пользователя. Требует аунтетификации.
    /// </summary>
    /// <param name="userID">Идентификатор пользователя для которого требуется получить аватрку.</param>
    /// <returns>Аватарка пользователя</returns>
    /// <response code="404">Указанный пользователь не нейден</response>
    /// <response code="400">Не найден аватар пользователя</response>
    /// <response code="409">Ошибка загрузки файла</response>
    [HttpGet("{userID}")]
    [Produces("image/png", "image/jpg")]
    [ProducesResponseType(typeof(byte[]), 200, "image/png", "image/jpg")]
    public async Task<IActionResult> GetImage(Guid userID)
    {
        var user = await _context.AuthUsers.FirstOrDefaultAsync(x => x.Id == userID);
        if (user == null)
            return NotFound();

        var stream = await _fileService.LoadFile("userimages", user.ImageSrc);

        return File(stream, $"image/{user.ImageSrc[(user.ImageSrc.IndexOf('.') + 1)..]}");
    }
}
