using System.Text;
using MailEase.Providers.Microsoft;
using Microsoft.Extensions.Configuration;

namespace MailEase.Tests.Providers.Azure;

public sealed class AzureCommunicationEmailTests : IClassFixture<ConfigurationFixture>
{
    private readonly IEmailProvider<AzureCommunicationEmailMessage> _emailProvider;
    private readonly IEmailProvider<AzureCommunicationEmailMessage> _emailProviderUsingEntraId;
    private readonly string _subject = "MailEase";
    private readonly string _from;
    private readonly string _to;

    public AzureCommunicationEmailTests(ConfigurationFixture fixture)
    {
        var config = fixture.Config;

        var connectionString =
            config.GetValue<string>("AZURE_COMMUNICATION_EMAIL_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Azure Communication Email connection string cannot be empty."
            );

        var tenantId =
            config.GetValue<string>("AZURE_TENANT_ID")
            ?? throw new InvalidOperationException("Azure tenant ID cannot be empty.");
        var clientId =
            config.GetValue<string>("AZURE_CLIENT_ID")
            ?? throw new InvalidOperationException("Azure client ID cannot be empty.");
        var clientSecret =
            config.GetValue<string>("AZURE_CLIENT_SECRET")
            ?? throw new InvalidOperationException("Azure client secret cannot be empty.");
        var endpoint =
            config.GetValue<string>("AZURE_COMMUNICATION_EMAIL_ENDPOINT")
            ?? throw new InvalidOperationException(
                "Azure Communication Email endpoint cannot be empty."
            );

        _subject = config.GetValue<string>("AZURE_COMMUNICATION_EMAIL_SUBJECT") ?? _subject;
        _from =
            config.GetValue<string>("AZURE_COMMUNICATION_EMAIL_FROM")
            ?? throw new InvalidOperationException("FROM cannot be empty.");
        _to =
            config.GetValue<string>("AZURE_COMMUNICATION_EMAIL_TO")
            ?? throw new InvalidOperationException("TO cannot be empty.");

        _emailProvider = Emails.AzureEmailCommunicationService(
            new AzureCommunicationParamsConnectionString(connectionString)
        );

        _emailProviderUsingEntraId = Emails.AzureEmailCommunicationService(
            new AzureCommunicationParamsEntraId(
                endpoint,
                new ClientSecretCredential(tenantId, clientId, clientSecret)
            )
        );
    }

    [Fact]
    public void SendEmail_WithEmptyConnectionString_ShouldThrowInvalidOperationException()
    {
        var azureCommunicationParamsFunc = () =>
            Emails.AzureEmailCommunicationService(new AzureCommunicationParamsConnectionString(""));
        azureCommunicationParamsFunc
            .Should()
            .Throw<InvalidOperationException>("Connection string cannot be empty.");
    }

    [Fact]
    public Task SendEmail_ShouldSucceed()
    {
        var request = new AzureCommunicationEmailMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Html = "<h1>Hello</h1>"
        };

        return _emailProvider.SendEmailAsync(request);
    }

    [Fact]
    public Task SendEmail_WithAttachment_ShouldSucceed()
    {
        var attachment = new EmailAttachment(
            "MyVerySecretAttachment.txt",
            new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")),
            "text/plain"
        );

        var request = new AzureCommunicationEmailMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Attachments = new List<EmailAttachment> { attachment },
            Html = "<h1>Hello</h1>"
        };

        return _emailProvider.SendEmailAsync(request);
    }

    #region EntraId

    [Fact]
    public Task SendEmailUsingEntraId_ShouldSucceed()
    {
        var request = new AzureCommunicationEmailMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Html = "<h1>Hello</h1>"
        };

        return _emailProviderUsingEntraId.SendEmailAsync(request);
    }

    #endregion
}
