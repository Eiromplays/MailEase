using System.Text.Json.Serialization;

namespace MailEase.Providers.Mailtrap;

public sealed class MailtrapRequest
{
    public required MailtrapEmailAddress From { get; init; }

    public required List<MailtrapEmailAddress> To { get; init; } = new();

    public List<MailtrapEmailAddress> Cc { get; init; } = new();

    public List<MailtrapEmailAddress> Bcc { get; init; } = new();

    public List<MailtrapAttachment> Attachments { get; init; } = new();

    public Dictionary<string, string> Headers { get; init; } = new();

    [JsonPropertyName("custom_variables")]
    public object? CustomVariables { get; init; }

    public required string Subject { get; init; }

    public string? Text { get; init; }

    public string? Html { get; init; }

    public string? Category { get; init; }
}

public sealed class MailtrapAttachment
{
    public required string Content { get; init; }

    public required string Type { get; init; }

    public required string Filename { get; init; }

    public required string Disposition { get; init; }

    [JsonPropertyName("content_id")]
    public string? ContentId { get; init; }
}
