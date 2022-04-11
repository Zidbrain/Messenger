using System.ComponentModel.DataAnnotations;

namespace Messenger.Models;

public record class ClientUserInfo(Guid ID, string Nickname, string? PhoneNumber, string? Status)
{
    public ClientUserInfo(AuthUser user) : this(user.Id, user.Nickname, user.PhoneNumber, user.Status)
    {
    }
}
