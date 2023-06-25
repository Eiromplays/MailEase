namespace MailEase.Results;

/// <summary>
/// Represents the result of sending an email.
/// </summary>
public class SendEmailResult
{
    /// <summary>
    /// The message ID of the email.
    /// </summary>
    public string MessageId { get; set; } = default!;
    
    /// <summary>
    /// The errors that occurred while sending the email.
    /// </summary>
    public IList<string> Errors { get; set; } = Array.Empty<string>();

    /// <summary>
    /// True if the email was sent successfully; otherwise, false.
    /// </summary>
    public bool IsSuccess => !Errors.Any();
}

public class SendEmailResult<T> : SendEmailResult
{
    /// <summary>
    /// The data returned from the email service.
    /// </summary>
    public T Data { get; set; } = default!;
}