using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Messenger.Models
{
    /// <summary>
    /// Сообщение
    /// </summary>
    public partial class Message
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        /// <summary>
        /// Пользователь, отправивший сообщение.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid UserFrom { get; set; }

        /// <summary>
        /// Пользователь, кому назначено сообщение.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid UserTo { get; set; }

        /// <summary>
        /// Тип сообщения.
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// Содержание сообщения.
        /// </summary>
        public string? Content { get; set; }

        [JsonIgnore]
        public bool IsDelivered { get; set; }

        /// <summary>
        /// Дата отправки сообщения.
        /// </summary>
        public DateTime DateSent { get; set; }

        [JsonIgnore]
        public virtual AuthUser UserFromNavigation { get; set; } = null!;
        [JsonIgnore]
        public virtual AuthUser UserToNavigation { get; set; } = null!;
    }
}
