using MailEase.Providers.Infobip;
using MailEase.Providers.SendGrid;

namespace MailEase;

public static class Emails
{
    public static IEmailProvider<SendGridMessage> SendGrid(SendGridParams sendGridParams) =>
        new SendGridEmailProvider(sendGridParams);

    public static IEmailProvider<InfobipMessage> Infobip(InfobipParams infobipParams) =>
        new InfobipEmailProvider(infobipParams);
}
