using System.Text.Json;
using System.Text.Json.Serialization;

namespace MailEase.Providers.Infobip;

public sealed class InfobipErrorResponse
{
    [JsonPropertyName("messageId")]
    public required string MessageId { get; init; }
    [JsonPropertyName("text")]
    public required string Text { get; init; }
        
    public List<JsonElement> ValidationErrors { get; init; } = new();
}

public sealed class InfobipValidationError
{
    public required string PropertyName { get; init; }
}