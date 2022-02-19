using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Messenger.Models
{
    public partial class Message
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid UserFrom { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid UserTo { get; set; }

        public MessageType MessageType { get; set; }
        public string? Content { get; set; }

        [JsonIgnore]
        public bool IsDelivered { get; set; }

        [JsonIgnore]
        public virtual AuthUser UserFromNavigation { get; set; } = null!;
        [JsonIgnore]
        public virtual AuthUser UserToNavigation { get; set; } = null!;
    }
}
