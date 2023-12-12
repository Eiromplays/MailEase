namespace MailEase;

public static class BaseEmailMessageErrors
{
    public static readonly MailEaseErrorDetail InvalidFromAddress =
        new(MailEaseErrorCode.InvalidFromAddress, "From address cannot be empty.");

    public static readonly MailEaseErrorDetail InvalidSubject =
        new(MailEaseErrorCode.InvalidSubject, "Subject cannot be empty.");

    public static readonly MailEaseErrorDetail NoRecipients =
        new(MailEaseErrorCode.NoRecipients, "To address cannot be empty.");

    public static readonly MailEaseErrorDetail InvalidToRecipients =
        new(MailEaseErrorCode.InvalidToRecipients, "One or more invalid recipients.");

    public static readonly MailEaseErrorDetail InvalidCcRecipients =
        new(MailEaseErrorCode.InvalidCcRecipients, "One or more invalid cc recipients.");

    public static readonly MailEaseErrorDetail InvalidBccRecipients =
        new(MailEaseErrorCode.InvalidBccRecipients, "One or more invalid bcc recipients.");

    public static readonly MailEaseErrorDetail InvalidReplyToRecipients =
        new(MailEaseErrorCode.InvalidReplyToRecipients, "One or more invalid reply-to recipients.");

    #region Provider specific errors

    public static MailEaseErrorDetail InvalidSendAt(string description) =>
        new(MailEaseErrorCode.InvalidSendAt, description);

    public static MailEaseErrorDetail InvalidBody(string description) =>
        new(MailEaseErrorCode.InvalidBody, description);

    public static MailEaseErrorDetail RecipientsExceedLimit(int maxRecipients) =>
        new(
            MailEaseErrorCode.RecipientsExceedLimit,
            $"Cannot have more than {maxRecipients} recipients.\n"
                + $"Please split the recipients into multiple emails.\n"
                + $"Or use the 'UseSplitting' parameter to enable (automatic) splitting."
        );

    #endregion
}
