using System.Net.Mail;
using MailEase.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MailEase.Smtp.Extensions;

public static class MailEaseSmtpBuilderExtensions
{
    public static MailEaseServicesBuilder AddSmtpEmailSender(this MailEaseServicesBuilder builder, SmtpClient smtpClient)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<IEmailSender>(_ => new SmtpEmailSender(smtpClient)));

        return builder;
    }
    
    public static MailEaseServicesBuilder AddSmtpEmailSender(this MailEaseServicesBuilder builder, Func<SmtpClient> smtpClientFactory)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<IEmailSender>(_ => new SmtpEmailSender(smtpClientFactory)));

        return builder;
    }
}