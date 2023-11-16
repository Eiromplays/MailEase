using MailEase.Providers.Microsoft;
using Microsoft.Extensions.Configuration;

namespace MailEase.Test.Providers;

public sealed class AzureCommunicationEmailTests
{
    private readonly IEmailProvider<AzureCommunicationEmailMessage> _emailProvider;
    private readonly string _subject = "MailEase";
    private readonly string _from;
    private readonly string _to;

    public AzureCommunicationEmailTests()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .Build();

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
    public void SendEmailWithEmptyApiKeyShouldThrow()
    {
        var azureCommunicationParamsFunc = () =>
            Emails.AzureEmailCommunicationService(new AzureCommunicationParams(""));
        azureCommunicationParamsFunc
            .Should()
            .Throw<ArgumentNullException>("Connection string cannot be empty.");
    }

    [Fact]
    public async Task SendEmail()
    {
        var request = new AzureCommunicationEmailMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Body = "<h1>Hello</h1>",
            IsHtmlBody = true
        };

        await _emailProvider.SendEmailAsync(request);
    }
}
