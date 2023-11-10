namespace MailEase.Providers.SendGrid;

public sealed record SendGridMessage : BaseEmailMessage
{
    public string? PlainTextBody { get; init; }

    public Dictionary<string, string> Headers { get; init; } = new();

    public bool SandBoxMode { get; init; }
}
