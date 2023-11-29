using System.Net.Http.Json;
using System.Text;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MailEase.Providers.Smtp;

namespace MailEase.Tests.Providers;

internal sealed class MailhogResponse
{
    public int Total { get; set; }

    public int Count { get; set; }

    public int Start { get; set; }
}

public sealed class SmtpTests : IAsyncLifetime
{
    private IEmailProvider<SmtpMessage> _emailProvider = null!;
    private IContainer _mailhogContainer = new ContainerBuilder()
        .WithImage("mailhog/mailhog")
        .WithPortBinding(SmtpPort)
        .WithPortBinding(HttpPort)
        .Build();
    private HttpClient _httpClient = null!;

    private const string Subject = "MailEase";
    private const string From = "john.doe@testcontainer";
    private const string To = "test@testcontainer";
    private const string Username = "john.doe";
    private const string Password = "secret";
    private const int SmtpPort = 1025;
    private const int HttpPort = 8025;

    public async Task InitializeAsync()
    {
        await _mailhogContainer.StartAsync();

        _emailProvider = Emails.Smtp(
            new SmtpParams(
                _mailhogContainer.IpAddress,
                _mailhogContainer.GetMappedPublicPort(SmtpPort),
                Username,
                Password
            )
        );

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(
                new Uri($"http://{_mailhogContainer.IpAddress}:{HttpPort}"),
                "/api/v2/"
            )
        };
    }

    [Fact]
    public void SendEmail_WithEmptyHost_ShouldThrowInvalidOperationException()
    {
        var smtpParamsFunc = () =>
            Emails.Smtp(new SmtpParams(string.Empty, SmtpPort, Username, Password));
        smtpParamsFunc.Should().Throw<InvalidOperationException>("Host cannot be empty.");
    }

    [Fact]
    public void SendEmail_WithEmptyUsername_ShouldThrowInvalidOperationException()
    {
        var smtpParamsFunc = () =>
            Emails.Smtp(
                new SmtpParams(_mailhogContainer.IpAddress, SmtpPort, string.Empty, Password)
            );
        smtpParamsFunc.Should().Throw<InvalidOperationException>("Username cannot be empty.");
    }

    [Fact]
    public void SendEmail_WithEmptyPassword_ShouldThrowInvalidOperationException()
    {
        var smtpParamsFunc = () =>
            Emails.Smtp(
                new SmtpParams(_mailhogContainer.IpAddress, SmtpPort, Username, string.Empty)
            );
        smtpParamsFunc.Should().Throw<InvalidOperationException>("Password cannot be empty.");
    }

    [Fact]
    public async Task SendEmail_ShouldSucceed()
    {
        var mailhogResponse = await GetMailhogMessagesAsync();

        mailhogResponse?.Total.Should().Be(0);

        var attachment = new EmailAttachment(
            "MyVerySecretAttachment.txt",
            new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")),
            "text/plain"
        );

        var request = new SmtpMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress>
            {
                new(To, "MailEase"),
                new("test2@testcontainer", "MailEase2")
            },
            Attachments = new List<EmailAttachment> { attachment },
            Html = "<h1>Hello</h1>"
        };

        await _emailProvider.SendEmailAsync(request);

        mailhogResponse = await GetMailhogMessagesAsync();

        mailhogResponse?.Total.Should().Be(1);
    }

    [Fact]
    public async Task SendEmail_WithAttachment_ShouldSucceed()
    {
        var mailhogResponse = await GetMailhogMessagesAsync();

        mailhogResponse?.Total.Should().Be(0);

        var attachment = new EmailAttachment(
            "MyVerySecretAttachment.txt",
            new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")),
            "text/plain"
        );

        var request = new SmtpMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new(To, "MailEase") },
            Attachments = new List<EmailAttachment> { attachment },
            Html = "<h1>Hello</h1>"
        };

        await _emailProvider.SendEmailAsync(request);

        mailhogResponse = await GetMailhogMessagesAsync();

        mailhogResponse?.Total.Should().Be(1);
    }

    private async Task<MailhogResponse?> GetMailhogMessagesAsync()
    {
        var response = await _httpClient.GetAsync("messages");

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MailhogResponse>();
    }

    public async Task DisposeAsync()
    {
        await _mailhogContainer.DisposeAsync();
    }
}
