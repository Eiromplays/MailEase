namespace MailEase;

public sealed record MailEaseErrorDetail(MailEaseErrorCode Code, string Description);

public enum MailEaseErrorCode
{
    Unknown = -1,
    GenericError = 0,
    NoRecipients,
    InvalidToRecipients,
    InvalidCcRecipients,
    InvalidBccRecipients,
    InvalidReplyToRecipients,
    InvalidSubject,
    InvalidFromAddress,
    InvalidBody,
    InvalidSendAt
    //...other error types
}
