using MailEase.Providers.Amazon;
using MailEase.Providers.Infobip;
using MailEase.Providers.Mailtrap;
using MailEase.Providers.Microsoft;
using MailEase.Providers.SendGrid;
using MailEase.Providers.Smtp;

namespace MailEase;

public static class Emails
{
    public static IEmailProvider<AzureCommunicationEmailMessage> AzureEmailCommunicationService(
        AzureCommunicationParamsConnectionString azureCommunicationParams
    ) => new AzureCommunicationEmailProvider(azureCommunicationParams);

    public static IEmailProvider<AzureCommunicationEmailMessage> AzureEmailCommunicationService(
        AzureCommunicationParamsEntraId azureCommunicationParams
    ) => new AzureCommunicationEmailProvider(azureCommunicationParams);
    
    public static IEmailProvider<AmazonSesMessage> AmazonSes(AmazonSesParams amazonSesParams) =>
        new AmazonSesEmailProvider(amazonSesParams);

    public static IEmailProvider<AmazonSesMessage> AmazonSesFromCliProfile(
        string profileName = CredentialFileParser.DefaultProfileName,
        string? region = null,
        string? version = null,
        string? path = null
    )
    {
        var parser = new CredentialFileParser();
        parser.FillCredentials(
            profileName,
            out var accessKeyId,
            out var secretAccessKey,
            out var sessionToken,
            out var configRegion
        );

        region ??= configRegion;

        if (region is null)
            throw new InvalidOperationException(
                "Region is required to be either passed as parameter or configured in AWS CLI config file"
            );

        return AmazonSes(
            new AmazonSesParams(accessKeyId, secretAccessKey, sessionToken, region, version, path)
        );
    }

    public static IEmailProvider<SendGridMessage> SendGrid(SendGridParams sendGridParams) =>
        new SendGridEmailProvider(sendGridParams);

    public static IEmailProvider<SmtpMessage> Smtp(SmtpParams smtpParams) =>
        new SmtpEmailProvider(smtpParams);

    public static IEmailProvider<InfobipMessage> Infobip(InfobipParams infobipParams) =>
        new InfobipEmailProvider(infobipParams);

    public static IEmailProvider<MailtrapMessage> Mailtrap(MailtrapParams mailtrapParams) =>
        new MailtrapEmailProvider(mailtrapParams);
}
