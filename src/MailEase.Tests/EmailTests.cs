using MailEase.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MailEase.Tests;

public class EmailTests
{
    [Fact]
    public void EmailBuilder_CorrectlySetsProperties()
    {
        // Arrange
        var expectedFromAddress = new EmailAddress("from@test.com");
        var expectedToAddress = new EmailAddress("to@test.com");
        const string expectedSubject = "Test Email";
        var expectedBody = new EmailBody("Hello, this is a test!");

        // Act
        var email = Email.CreateBuilder()
            .From(expectedFromAddress)
            .To(expectedToAddress)
            .Subject(expectedSubject)
            .Body(expectedBody)
            .Build();

        // Assert
        Assert.Equal(expectedFromAddress, email.Data.From);
        Assert.Contains(expectedToAddress, email.Data.To);
        Assert.Equal(expectedSubject, email.Data.Subject);
        Assert.Equal(expectedBody, email.Data.Body);
    }

    [Fact]
    public void EmailBuilder_CorrectlyBuildsEmail()
    {
        // Arrange & Act
        var email = Email.CreateBuilder()
            .From("from@test.com")
            .To("to@test.com")
            .Subject("Hello")
            .Body("Welcome to xUnit Testing")
            .Build();

        // Assert
        Assert.NotNull(email);
        Assert.IsAssignableFrom<IMailEaseEmail>(email);
    }
    
    [Fact]
    public void EmailBuilder_CorrectlySetsCC()
    {
        // Arrange
        var expectedCcAddress = new EmailAddress("cc@test.com");

        // Act
        var email = Email.CreateBuilder()
            .From("from@test.com")
            .To("to@test.com")
            .Subject("Hello")
            .Body("Welcome to xUnit Testing")
            .Cc(expectedCcAddress)
            .Build();

        // Assert
        Assert.Contains(expectedCcAddress, email.Data.Cc);
    }

    [Fact]
    public void EmailBuilder_CorrectlySetsBCC()
    {
        // Arrange
        var expectedBccAddress = new EmailAddress("bcc@test.com");

        // Act
        var email = Email.CreateBuilder()
            .From("from@test.com")
            .To("to@test.com")
            .Subject("Hello")
            .Body("Welcome to xUnit Testing")
            .Bcc(expectedBccAddress)
            .Build();

        // Assert
        Assert.Contains(expectedBccAddress, email.Data.Bcc);
    }

    [Fact]
    public void EmailBuilder_CorrectlySetsReplyTo()
    {
        // Arrange
        var expectedReplyToAddress = new EmailAddress("replyto@test.com");

        // Act
        var email = Email.CreateBuilder()
            .From("from@test.com")
            .To("to@test.com")
            .Subject("Hello")
            .Body("Welcome to xUnit Testing")
            .ReplyTo(expectedReplyToAddress)
            .Build();

        // Assert
        Assert.Contains(expectedReplyToAddress, email.Data.ReplyTo);
    }

    /*[Fact]
    public void EmailBuilder_CorrectlySetsAttachments()
    {
        // Arrange
        var expectedAttachment = new EmailAttachment();

        // Act
        var email = Email.CreateBuilder()
            .From("from@test.com")
            .To("to@test.com")
            .Subject("Hello")
            .Body("Welcome to xUnit Testing")
            .Attachments(new List<EmailAttachment> { expectedAttachment })
            .Build();

        // Assert
        Assert.Contains(expectedAttachment, email.Data.Attachments);
    }*/

    [Fact]
    public void EmailBuilder_CorrectlySetsHeaders()
    {
        // Arrange
        var expectedHeaderKey = "headerKey";
        var expectedHeaderValue = "headerValue";

        // Act
        var email = Email.CreateBuilder()
            .From("from@test.com")
            .To("to@test.com")
            .Subject("Hello")
            .Body("Welcome to xUnit Testing")
            .Headers(new Dictionary<string, string> { { expectedHeaderKey, expectedHeaderValue } })
            .Build();

        // Assert
        Assert.True(email.Data.Headers.ContainsKey(expectedHeaderKey));
        Assert.Equal(expectedHeaderValue, email.Data.Headers[expectedHeaderKey]);
    }

    [Fact]
    public void EmailBuilder_UsingDI_CorrectlySetsProperties()
    {
        // Arrange
        var expectedFromAddress = new EmailAddress("test@test.com", "Test");
        var services = new ServiceCollection()
            .AddMailEase("test@test.com", "Test")
            .Services
            .BuildServiceProvider(true);
        
        var emailBuilderFactory = services.GetRequiredService<IEmailBuilderFactory>();

        // Act
        var email = emailBuilderFactory.From("default")
            .From(expectedFromAddress)
            .To("test@test2.com")
            .Subject("Test Email")
            .Body("Hello, this is a test!")
            .Build();

        // Assert
        Assert.Equal(expectedFromAddress, email.Data.From);
    }
}