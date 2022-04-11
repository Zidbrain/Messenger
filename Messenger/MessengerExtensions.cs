using Messenger.Services;
using Messenger.Controllers;
using Minio;

namespace Messenger;

public static class MessengerExtensions
{
    public static IServiceCollection AddMessenger(this IServiceCollection collection) =>
        collection.AddSingleton(new MessengingService());

    public static IServiceCollection AddMinioFileService(this IServiceCollection collection) =>
        collection.AddScoped(provider => new MinioFileServiceFactory());
}
