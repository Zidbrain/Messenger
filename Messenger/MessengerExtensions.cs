using Messenger.Services;
using Messenger.Controllers;

namespace Messenger;

public static class MessengerExtensions
{
    public static IServiceCollection AddMessenger(this IServiceCollection collection) =>
        collection.AddSingleton(new MessengingService());
}
