using Minio;

namespace Messenger.Services;

public interface IFileService
{
    Task SaveFile(string folder, string fileName, Stream fileContents);
    Task<Stream> LoadFile(string folder, string fileName);
}
