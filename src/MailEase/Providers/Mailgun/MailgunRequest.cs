namespace MailEase.Providers.Mailgun;

public sealed class MailgunRequest
{
    public required string From { get; init; }

    public required List<string> To { get; init; } = new();

    public List<string> Cc { get; init; } = new();

    public List<string> Bcc { get; init; } = new();

    public required string Subject { get; init; }

    public string? Text { get; init; }

    public string? Html { get; init; }
}
