namespace MailEase;

public sealed record MailEaseErrorDetail(MailEaseErrorCode Code, string Description);

/// <summary>
/// A list of all error codes. See <see cref="MailEaseErrorCode"/>
/// </summary>
public enum MailEaseErrorCode
{
    /// <summary>
    /// This code signifies that an Unknown error has occurred.
    /// Probably a exception/error thrown/returned by the provider
    /// </summary>
    Unknown = -1,

    /// <summary>
    /// This code is returned when there are no recipients specified in the email.
    /// </summary>
    NoRecipients,

    /// <summary>
    /// This code is returned when the provided 'ToRecipients' are not valid.
    /// </summary>
    InvalidToRecipients,

    /// <summary>
    /// This code is returned when the provided 'CcRecipients' are not valid.
    /// </summary>
    InvalidCcRecipients,

    /// <summary>
    /// This code is returned when the provided 'BccRecipients' are not valid.
    /// </summary>
    InvalidBccRecipients,

    /// <summary>
    /// This code is returned when the provided 'ReplyToRecipients' are not valid.
    /// </summary>
    InvalidReplyToRecipients,

    /// <summary>
    /// This code is returned when the provided 'Subject' is not valid.
    /// </summary>
    InvalidSubject,

    /// <summary>
    /// This code is returned when the provided 'FromAddress' is not valid.
    /// </summary>
    InvalidFromAddress,

    /// <summary>
    /// This code is returned when the provided 'Body' of the email is not valid.
    /// Most likely due to not providing valid 'Text' or 'Html' properties.
    /// </summary>
    InvalidBody,

    /// <summary>
    /// This code is returned when the provided 'SendAt' date or time is not valid.
    /// </summary>
    InvalidSendAt,
    
    /// <summary>
    /// This code is returned when the total recipients exceed the provider limit.
    /// And the 'UseSplitting' property is set to <c>false</c>.
    /// </summary>
    /// <remarks>
    /// See <see cref="BaseEmailMessage.UseSplitting"/>
    /// </remarks>
    RecipientsExceedLimit
}
