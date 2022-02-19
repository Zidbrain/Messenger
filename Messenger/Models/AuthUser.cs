using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Messenger.Models
{
    /// <summary>
    /// Информация о пользователе
    /// </summary>
    public partial class AuthUser
    {
        public AuthUser()
        {
            MessageUserFromNavigations = new HashSet<Message>();
            MessageUserToNavigations = new HashSet<Message>();
        }

        public AuthUser(string username, string password) : this()
        {
            Id = Guid.NewGuid();
            Username = username;
            Password = GetHash(password);
        }

        public static byte[] GetHash(string password) =>
             SHA256.HashData(Encoding.UTF8.GetBytes(password));
        
        /// <summary>
        /// UUID пользователя.
        /// </summary>
        [Required]
        public Guid Id { get; set; }
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        [Required]
        public string Username { get; set; } = null!;

        [JsonIgnore]
        public byte[] Password { get; set; } = null!;

        [JsonIgnore]
        public string ImageSrc { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<Message> MessageUserFromNavigations { get; set; }
        [JsonIgnore]
        public virtual ICollection<Message> MessageUserToNavigations { get; set; }
    }
}
