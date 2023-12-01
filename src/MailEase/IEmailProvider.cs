namespace MailEase;

public interface IEmailProvider<in TEmailMessage>
    where TEmailMessage : IEmailMessage
{
    Task<EmailResponse> SendEmailAsync(
        TEmailMessage request,
        CancellationToken cancellationToken = default
    );
}

public record EmailResponse(bool IsSuccess, string[]? MessageIds = null);
