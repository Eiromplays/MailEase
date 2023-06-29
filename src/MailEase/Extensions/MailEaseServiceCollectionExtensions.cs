using System.Collections.Concurrent;
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
        
        config.FromAddresses.TryAdd("default", config.DefaultFrom);
        
        services.TryAddTransient<IEmailBuilderFactory>(provider =>
            new EmailBuilderFactory(provider.GetService<IEmailSender>(), config.FromAddresses));

        return builder;
    }
    
    public static MailEaseServicesBuilder AddMailEase(this IServiceCollection services, string defaultFromAddress, string? defaultFromName = null, ConcurrentDictionary<string, EmailAddress>? fromEmailAddresses = null) =>
        AddMailEase(services, configuration =>
        {
            configuration.DefaultFrom = new EmailAddress(defaultFromAddress, defaultFromName);
            configuration.FromAddresses.AddRange(fromEmailAddresses ?? new ConcurrentDictionary<string, EmailAddress>());
        });
}

public sealed class MailEaseServicesBuilder
{
    public IServiceCollection Services { get; }

    internal MailEaseServicesBuilder(IServiceCollection services)
    {
        Services = services;
    }
}