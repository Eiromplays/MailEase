namespace MailEase.Providers.SendGrid;

public sealed record SendGridMessage : BaseEmailMessage
{
    public string? PlainTextBody { get; init; }

    public Dictionary<string, string> Headers { get; init; } = new();

    public string? Template { get; init; }

    public bool SandBoxMode { get; init; }
}
