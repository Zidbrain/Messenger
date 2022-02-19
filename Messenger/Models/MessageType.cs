using System.Text.Json.Serialization;

namespace Messenger.Models;

/// <summary>
/// Тип контента сообщения
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageType
{
    /// <summary>
    /// Текстовое сообщение
    /// </summary>
    Text = 0,
    /// <summary>
    /// Сообщение содержит файл
    /// </summary>
    File = 1,
}
