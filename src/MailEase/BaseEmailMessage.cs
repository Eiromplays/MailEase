namespace MailEase;

public abstract record BaseEmailMessage : IEmailMessage
{
    public required string Subject { get; init; }
    public required EmailAddress From { get; init; }
    public required List<EmailAddress> ToAddresses { get; init; } = new();
    public List<EmailAddress> CcAddresses { get; init; } = new();
    public List<EmailAddress> BccAddresses { get; init; } = new();
    public List<EmailAddress> ReplyToAddresses { get; init; } = new();
    public required string Body { get; init; }
    public string? PlainTextBody { get; init; }
    public bool IsHtmlBody { get; init; }
    public Dictionary<string, string> Headers { get; init; } = new();

    public bool SandBoxMode { get; init; }

    public string? TemplateId { get; init; }

    public DateTimeOffset? SendAt { get; init; }
}
