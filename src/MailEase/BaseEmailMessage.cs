namespace MailEase;

public interface IEmailMessage
{
    string Subject { get; init; }
    EmailAddress From { get; init; }
    List<EmailAddress> ToAddresses { get; init; }
    List<EmailAddress> CcAddresses { get; init; }
    List<EmailAddress> BccAddresses { get; init; }
    List<EmailAddress> ReplyToAddresses { get; init; } 
    string? Text { get; init; }
    string? Html { get; init; }
    List<EmailAttachment> Attachments { get; init; }
    DateTimeOffset? SendAt { get; init; }
}

public abstract record BaseEmailMessage : IEmailMessage
{
    public required string Subject { get; init; }
    public required EmailAddress From { get; init; }
    public required List<EmailAddress> ToAddresses { get; init; } = [];
    public List<EmailAddress> CcAddresses { get; init; } = [];
    public List<EmailAddress> BccAddresses { get; init; } = [];
    public List<EmailAddress> ReplyToAddresses { get; init; } = [];
    
    public string? Text { get; init; }
    public string? Html { get; init; }

    public List<EmailAttachment> Attachments { get; init; } = [];

    public DateTimeOffset? SendAt { get; init; }
}
