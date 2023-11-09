namespace MailEase;

public interface IEmailProvider<in TRequest> : IDisposable
    where TRequest : IEmailMessage
{
    Task SendEmailAsync(TRequest request, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
