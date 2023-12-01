using System.Text.Json.Serialization;

namespace MailEase.Providers.Mailtrap;

public sealed class MailtrapResponse
{
    public bool Success { get; init; }

    [JsonPropertyName("message_ids")]
    public List<string> MessageIds { get; init; } = new();
}
