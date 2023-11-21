namespace MailEase.Providers.Mailgun;

public sealed record MailgunMessage : BaseEmailMessage
{
    public string? PlainTextBody { get; init; }

    public Dictionary<string, string> Headers { get; init; } = new();
}
