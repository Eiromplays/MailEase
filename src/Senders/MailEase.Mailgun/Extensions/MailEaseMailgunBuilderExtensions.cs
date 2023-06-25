using MailEase.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MailEase.Mailgun.Extensions;

public static class MailEaseMailgunBuilderExtensions
{
    public static MailEaseServicesBuilder AddMailgunEmailSender(this MailEaseServicesBuilder builder, string domainName, string apiKey, MailGunRegion region = MailGunRegion.Usa) =>
        AddMailgunEmailSender(builder, new MailgunConfiguration { ApiKey = apiKey, DomainName = domainName, Region = region });

    public static MailEaseServicesBuilder AddMailgunEmailSender(this MailEaseServicesBuilder builder, MailgunConfiguration mailgunConfiguration)
    {
        builder.Services.TryAdd(ServiceDescriptor.Scoped<IEmailSender>(_ => new MailgunEmailSender(mailgunConfiguration)));
        return builder;
    }
}