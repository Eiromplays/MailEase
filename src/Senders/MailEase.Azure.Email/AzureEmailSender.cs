using Azure;
using Azure.Communication.Email;
using MailEase.Azure.Email.Extensions;
using MailEase.Extensions;
using MailEase.Results;

namespace MailEase.Azure.Email;

// Read more about Azure Email Communication Services here:
// https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/email/send-email?pivots=programming-language-csharp
public class AzureEmailSender : IEmailSender
{
    private readonly EmailClient _emailClient;
    
    /// <summary>
    /// Uses the provided <see cref="EmailClient"/> to send emails.
    /// </summary>
    /// <param name="emailClient">The <see cref="EmailClient"/> to use for sending emails.</param>
    public AzureEmailSender(EmailClient emailClient)
    {
        _emailClient = emailClient;
    }

    public async Task<SendEmailResult> SendAsync(IMailEaseEmail email, CancellationToken cancellationToken = default)
    {
        var result = new SendEmailResult();
        
        var emailContent = new EmailContent(email.Data.Subject);
        
        if (email.Data.Body.IsHtml)
            emailContent.Html = email.Data.Body.Content;
        else
            emailContent.PlainText = email.Data.Body.Content;
        
        var toAddresses = email.Data.To.Select(x => x.ToAzureEmailAddress()).ToList();
        var ccAddresses = email.Data.Cc.Select(x => x.ToAzureEmailAddress()).ToList();
        var bccAddresses = email.Data.Bcc.Select(x => x.ToAzureEmailAddress()).ToList();

        var emailRecipients = new EmailRecipients(toAddresses, ccAddresses, bccAddresses);
        
        var emailMessage = new EmailMessage(email.Data.From.ToString(), emailRecipients, emailContent);
        
        emailMessage.ReplyTo.AddRange(email.Data.ReplyTo.Select(x => x.ToAzureEmailAddress()));
        emailMessage.Headers.AddRange(email.Data.Headers);
        emailMessage.Attachments.AddRange(email.Data.Attachments.Select(x =>
            new global::Azure.Communication.Email.EmailAttachment(x.FileName, x.ContentType, new BinaryData(x.Data))));

        try
        {
            var emailSendOperation = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage, cancellationToken);
            
            result.MessageId = emailSendOperation.Id;
        }
        catch (RequestFailedException ex)
        {
            result.Errors.Add($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
        }
        
        return result;
    }
}