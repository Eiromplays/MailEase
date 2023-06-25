using MailEase.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MailEase.Mailtrap.Extensions;

public static class MailEaseMailtrapBuilderExtensions
{
    public static MailEaseServicesBuilder AddMailtrapEmailSender(this MailEaseServicesBuilder builder, string userName,
        string password, string host = "smtp.mailtrap.io", int? port = null, bool enableSsl = true) =>
        AddMailtrapEmailSender(builder,
            new MailtrapConfiguration
                { UserName = userName, Password = password, Host = host, Port = port, EnableSsl = enableSsl });

    public static MailEaseServicesBuilder AddMailtrapEmailSender(this MailEaseServicesBuilder builder, MailtrapConfiguration configuration)
    {
        builder.Services.TryAdd(ServiceDescriptor.Scoped<IEmailSender>(_ => new MailtrapEmailSender(configuration)));

        return builder;
    }
}