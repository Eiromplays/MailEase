using MailEase.Results;

namespace MailEase;

/// <summary>
/// Send an email.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Synchronously send an email.
    /// </summary>
    /// <param name="email">The email to send.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="SendEmailResult"/> indicating the result of the operation.</returns>
    SendEmailResult Send(IMailEaseEmail email, CancellationToken cancellationToken = default) =>
        SendAsync(email, cancellationToken).GetAwaiter().GetResult();
    
    /// <summary>
    /// Asynchronously send an email.
    /// </summary>
    /// <param name="email">The email to send.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="SendEmailResult"/> indicating the result of the operation.</returns>
    Task<SendEmailResult> SendAsync(IMailEaseEmail email, CancellationToken cancellationToken = default);
}