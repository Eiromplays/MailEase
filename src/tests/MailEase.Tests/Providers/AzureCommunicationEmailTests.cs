using System.Text;
using MailEase.Providers.Microsoft;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MailEase.Tests.Providers;

public sealed class AzureCommunicationEmailTests : IClassFixture<ConfigurationFixture>
{
    private readonly IEmailProvider<AzureCommunicationEmailMessage> _emailProvider;
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

        _subject = config.GetValue<string>("AZURE_COMMUNICATION_EMAIL_SUBJECT") ?? _subject;
        _from =
            config.GetValue<string>("AZURE_COMMUNICATION_EMAIL_FROM")
            ?? throw new InvalidOperationException("FROM cannot be empty.");
        _to =
            config.GetValue<string>("AZURE_COMMUNICATION_EMAIL_TO")
            ?? throw new InvalidOperationException("TO cannot be empty.");

        _emailProvider = Emails.AzureEmailCommunicationService(
            new AzureCommunicationParams(connectionString)
        );
    }

    [Fact]
    public void SendEmail_WithEmptyConnectionString_ShouldThrowInvalidOperationException()
    {
        var azureCommunicationParamsFunc = () =>
            Emails.AzureEmailCommunicationService(new AzureCommunicationParams(""));
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
            Body = "<h1>Hello</h1>",
            IsHtmlBody = true
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
            Body = "<h1>Hello</h1>",
            IsHtmlBody = true
        };

        return _emailProvider.SendEmailAsync(request);
    }
}
