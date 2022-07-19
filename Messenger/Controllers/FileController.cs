using Messenger.Services;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.Exceptions;

namespace Messenger.Controllers;

[Route("api/file")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly MessengerContext _context;

    public FileController(MinioFileServiceFactory factory, MessengerContext context)
    {
        _fileService = factory.CreateInstance(new MinioClient()
            .WithCredentials(MinioFileService.FilesCredentials, MinioFileService.FilesCredentials)
            .WithEndpoint(MinioFileService.AccessURL)
            .Build());

        _context = context;
    }

    private record class FileHandleJSON(Guid FileHandle);

    /// <summary>
    /// Получить файл с заданным индетификатором.
    /// </summary>
    /// <param name="fileHandle">Индетификатор файла</param>
    /// <returns>Файл</returns>
    /// <response code="200">Файл</response>
    /// <response code="404">Файл не найден</response>
    [HttpGet]
    [ProducesResponseType(typeof(byte[]), 200, "application/octet-stream")]
    public async Task<IActionResult> Get(Guid fileHandle)
    {
        try
        {
            var file = await _fileService.LoadFile("filestorage", fileHandle.ToString());
            var filename = await _context.FileNames.FirstAsync(fn => fn.ID == fileHandle);

            return File(file, "application/octet-stream", fileDownloadName: filename.Name);
        }
        catch (ObjectNotFoundException ex)
        {
            return NotFound(ex.ToString());
        }
    }

    /// <summary>
    /// Загрузить файл для сообщения. Файл должен быть загружен с помощью form-data.
    /// </summary>
    /// <param name="file">Поле с именем 'File' в form-data</param>
    /// <returns>Уникальный id файла который используется для последуюзего обращения к нему</returns>
    /// <response code="200">Уникальный id файла который используется для последуюзего обращения к нему</response>
    [HttpPost]
    [ProducesResponseType(typeof(FileHandleJSON), 200)]
    public async Task<IActionResult> Post(IFormFile file)
    {
        var filename = new FileName(Guid.NewGuid(), file.FileName);
        using var stream = file.OpenReadStream();

        await _fileService.SaveFile("filestorage", filename.ID.ToString(), stream);
        await _context.FileNames.AddAsync(filename);
        await _context.SaveChangesAsync();

        return Ok(new FileHandleJSON(filename.ID));
    }
}
