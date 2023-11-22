namespace MailEase;

public interface IEmailMessage
{
    string Subject { get; init; }
    EmailAddress From { get; init; }
    List<EmailAddress> ToAddresses { get; init; }
    List<EmailAddress> CcAddresses { get; init; }
    List<EmailAddress> BccAddresses { get; init; }
    List<EmailAddress> ReplyToAddresses { get; init; }
    string Body { get; init; }
    bool IsHtmlBody { get; init; }
    List<EmailAttachment> Attachments { get; init; }
    DateTimeOffset? SendAt { get; init; }
}

public abstract record BaseEmailMessage : IEmailMessage
{
    public required string Subject { get; init; }
    public required EmailAddress From { get; init; }
    public required List<EmailAddress> ToAddresses { get; init; } = new();
    public List<EmailAddress> CcAddresses { get; init; } = new();
    public List<EmailAddress> BccAddresses { get; init; } = new();
    public List<EmailAddress> ReplyToAddresses { get; init; } = new();
    public required string Body { get; init; }
    public bool IsHtmlBody { get; init; }

    public List<EmailAttachment> Attachments { get; init; } = new();

    public DateTimeOffset? SendAt { get; init; }
}
