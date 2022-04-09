using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Messenger.Controllers;
[Route("file")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FileController : ControllerBase
{
    /// <summary>
    /// Получить файл с заданным индетификатором.
    /// </summary>
    /// <param name="fileHandle">Индетификатор файла</param>
    /// <returns>Файл</returns>
    /// <response code="200">Файл</response>
    [HttpGet]
    
    public async Task<IActionResult> Get(string fileHandle)
    {
        return File(null as byte[], "*/*");
    }

    /// <summary>
    /// Загрузить файл для сообщения. Файл должен быть загружен с помощью form-data.
    /// </summary>
    /// <param name="file">Поле с именем 'File' в form-data</param>
    /// <returns>Уникальный id файла который используется для последуюзего обращения к нему</returns>
    /// <response code="200">Уникальный id файла который используется для последуюзего обращения к нему</response>
    [HttpPost]
    public async Task<IActionResult> Post([FromForm(Name ="File")] IFormFile file)
    {
        return Ok();
    }
}
