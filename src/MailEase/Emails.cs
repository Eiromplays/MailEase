using MailEase.Providers.Infobip;
using MailEase.Providers.Mailtrap;
using MailEase.Providers.Microsoft;
using MailEase.Providers.SendGrid;

namespace MailEase;

public static class Emails
{
    public static IEmailProvider<AzureCommunicationEmailMessage> AzureEmailCommunicationService(
        AzureCommunicationParams azureCommunicationParams
    ) => new AzureCommunicationEmailProvider(azureCommunicationParams);

    public static IEmailProvider<SendGridMessage> SendGrid(SendGridParams sendGridParams) =>
        new SendGridEmailProvider(sendGridParams);

    public static IEmailProvider<InfobipMessage> Infobip(InfobipParams infobipParams) =>
        new InfobipEmailProvider(infobipParams);

    public static IEmailProvider<MailtrapMessage> Mailtrap(MailtrapParams mailtrapParams) =>
        new MailtrapEmailProvider(mailtrapParams);
}
