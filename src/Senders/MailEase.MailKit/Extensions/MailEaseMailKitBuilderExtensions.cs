using MailEase.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MailEase.MailKit.Extensions;

public static class MailEaseMailKitBuilderExtensions
{
    public static MailEaseServicesBuilder AddMailKitEmailSender(this MailEaseServicesBuilder builder, MailKitConfiguration mailKitConfiguration)
    {
        builder.Services.TryAdd(ServiceDescriptor.Scoped<IEmailSender>(_ => new MailKitEmailSender(mailKitConfiguration)));
        return builder;
    }
}