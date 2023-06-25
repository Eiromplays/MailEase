using MailEase.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SendGrid;

namespace MailEase.SendGrid.Extensions;

public static class MailEaseSendGridBuilderExtensions
{
    public static MailEaseServicesBuilder AddSendGridEmailSender(this MailEaseServicesBuilder builder, string apiKey) =>
        AddSendGridEmailSender(builder, new SendGridClient(apiKey));

    public static MailEaseServicesBuilder AddSendGridEmailSender(this MailEaseServicesBuilder builder, SendGridClient sendGridClient)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<IEmailSender>(_ => new SendGridEmailSender(sendGridClient)));

        return builder;
    }
}