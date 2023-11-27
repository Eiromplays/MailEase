using System.Text;
using MailEase.Providers.Amazon;
using Microsoft.Extensions.Configuration;

namespace MailEase.Tests.Providers;

public sealed class AmazonSesTests : IClassFixture<ConfigurationFixture>
{
    private readonly IEmailProvider<AmazonSesMessage> _emailProvider;
    private readonly string _subject = "MailEase";
    private readonly string _from;
    private readonly string _to;

    public AmazonSesTests(ConfigurationFixture fixture)
    {
        var config = fixture.Config;

        var accessKeyId =
            config.GetValue<string>("AMAZON_SES_ACCESS_KEY_ID")
            ?? throw new InvalidOperationException("Access key ID cannot be empty.");

        var secretAccessKey =
            config.GetValue<string>("AMAZON_SES_SECRET_ACCESS_KEY")
            ?? throw new InvalidOperationException("Secret access key cannot be empty.");

        var region =
            config.GetValue<string>("AMAZON_SES_REGION")
            ?? throw new InvalidOperationException("Region cannot be empty.");

        _subject = config.GetValue<string>("AMAZON_SES_SUBJECT") ?? _subject;
        _from =
            config.GetValue<string>("AMAZON_SES_FROM")
            ?? throw new InvalidOperationException("FROM cannot be empty.");
        _to =
            config.GetValue<string>("AMAZON_SES_TO")
            ?? throw new InvalidOperationException("TO cannot be empty.");

        _emailProvider = Emails.AmazonSes(
            new AmazonSesParams(accessKeyId, secretAccessKey, region)
        );
    }

    [Fact]
    public void SendEmail_WithEmptyAccessKeyId_ShouldThrowInvalidOperationException()
    {
        var amazonSesParamsFunc = () =>
            Emails.AmazonSes(new AmazonSesParams("", "secret", "us-east-1"));
        amazonSesParamsFunc.Should().Throw<ArgumentNullException>("Access key ID cannot be empty.");
    }

    [Fact]
    public void SendEmail_WithEmptySecretAccessKey_ShouldThrowInvalidOperationException()
    {
        var amazonSesParamsFunc = () =>
            Emails.AmazonSes(new AmazonSesParams("key", "", "us-east-1"));
        amazonSesParamsFunc
            .Should()
            .Throw<ArgumentNullException>("Secret access key cannot be empty.");
    }

    [Fact]
    public void SendEmail_WithEmptyRegion_ShouldThrowInvalidOperationException()
    {
        var amazonSesParamsFunc = () => Emails.AmazonSes(new AmazonSesParams("key", "secret", ""));
        amazonSesParamsFunc.Should().Throw<ArgumentNullException>("Region cannot be empty.");
    }

    [Fact]
    public Task SendEmail_ShouldSucceed()
    {
        var request = new AmazonSesMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to) },
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

        var request = new AmazonSesMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Attachments = new List<EmailAttachment> { attachment },
            Html = "<h1>Hello</h1>"
        };

        return _emailProvider.SendEmailAsync(request);
    }
}
