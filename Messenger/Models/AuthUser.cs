using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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
            Username = username;
            Salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(6));
            Password = GetHash(password);
        }

        public bool CheckHash(string password, out byte[] hashedPassword)
        {
            hashedPassword = GetHash(password);
            return hashedPassword.SequenceEqual(Password);
        }

        public byte[] GetHash(string password) => 
            SHA512.HashData(Encoding.Unicode.GetBytes(password.Insert(password.Length / 2, Salt)));

        /// <summary>
        /// Индетификатор пользователя
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Логин пользователя
        /// </summary>
        [JsonIgnore]
        public string Username { get; set; } = null!;

        [JsonIgnore]
        public byte[] Password { get; set; } = null!;

        [JsonIgnore]
        public string Salt { get; set; } = null!;

        [Required]
        public string Nickname { get; set; } = null!;

        [JsonIgnore]
        public string ImageSrc { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Status { get; set; }

        [JsonIgnore]
        public virtual ICollection<Message> MessageUserFromNavigations { get; set; }
        [JsonIgnore]
        public virtual ICollection<Message> MessageUserToNavigations { get; set; }
    }
}
