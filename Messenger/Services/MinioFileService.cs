using Minio;

namespace Messenger.Services;

public class MinioFileService : IFileService
{
    public MinioClient Client { get; set; }

    public MinioFileService(MinioClient client) =>
        Client = client;

    public async Task<Stream> LoadFile(string folder, string fileName)
    {
        var stream = new MemoryStream();
        var image = await Client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(folder)
            .WithObject(fileName)
            .WithCallbackStream(s => s.CopyTo(stream)));
        stream.Position = 0;

        return stream;
    }

    public async Task SaveFile(string folder, string fileName, Stream fileContents)
    {
        await Client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(folder)
            .WithObject(fileName)
            .WithStreamData(fileContents)
            .WithObjectSize(fileContents.Length));
    }
}

public class MinioFileServiceFactory
{
    public MinioFileService CreateInstance(MinioClient client) =>
        new(client);
}
