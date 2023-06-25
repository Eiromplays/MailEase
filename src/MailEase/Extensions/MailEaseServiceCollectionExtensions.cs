using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MailEase.Extensions;

public static class MailEaseServiceCollectionExtensions
{
    public static MailEaseServicesBuilder AddMailEase(this IServiceCollection services, Action<MailEaseConfiguration> configuration)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));
        
        var config = new MailEaseConfiguration();
        configuration.Invoke(config);
        
        var builder = new MailEaseServicesBuilder(services);
        services.TryAdd(ServiceDescriptor.Transient<IMailEaseEmail>(x => new Email(new EmailAddress(
                config.DefaultFromAddress,
                config.DefaultFromName),
            x.GetService<IEmailSender>()!)));

        return builder;
    }
}

public sealed class MailEaseServicesBuilder
{
    public IServiceCollection Services { get; }

    internal MailEaseServicesBuilder(IServiceCollection services)
    {
        Services = services;
    }
}