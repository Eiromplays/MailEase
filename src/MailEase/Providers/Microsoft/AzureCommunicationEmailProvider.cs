using MailEase.Exceptions;

namespace MailEase.Providers.Microsoft;

public sealed class AzureCommunicationEmailProvider : BaseEmailProvider<BaseEmailMessage>
{
    public AzureCommunicationEmailProvider(
        Uri endpoint,
        DelegatingHandler authenticationHandler,
        string apiVersion = "2023-03-31s"
    )
        : base(endpoint, authenticationHandler) { }

    public override Task SendEmailAsync(
        BaseEmailMessage message,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }
}
