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
    string? PlainTextBody { get; init; }
    bool IsHtmlBody { get; init; }
    Dictionary<string, string> Headers { get; init; }
}
