using MailEase.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace MailEase.Graph.Extensions;

public static class MailEaseGraphBuilderExtensions
{
    public static MailEaseServicesBuilder AddGraphEmailSender(this MailEaseServicesBuilder builder,
        IAuthenticationProvider authenticationProvider, bool saveSentEmails = false) =>
        builder.AddGraphEmailSender(new GraphConfiguration { AuthenticationProvider = authenticationProvider, SaveSentEmails = saveSentEmails });

    public static MailEaseServicesBuilder AddGraphEmailSender(this MailEaseServicesBuilder builder, GraphConfiguration graphConfiguration)
    {
        builder.Services.TryAdd(ServiceDescriptor.Scoped<IEmailSender>(_ => new GraphEmailSender(graphConfiguration)));
        return builder;
    }
}