using MailEase.Extensions;
using MailEase.Results;
using MailEase.SendGrid.Extensions;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MailEase.SendGrid;

public class SendGridEmailSender : IEmailSender
{
    private readonly SendGridClient _sendGridClient;

    public SendGridEmailSender(SendGridClient sendGridClient)
    {
        _sendGridClient = sendGridClient;
    }

    public async Task<SendEmailResult> SendAsync(IMailEaseEmail email, CancellationToken cancellationToken = default)
    {
        var sendGridResponse = await _sendGridClient.SendEmailAsync(await CreateSendGridMessageAsync(email, cancellationToken), cancellationToken);
        
        var result = new SendEmailResult();
        if (sendGridResponse.Headers.TryGetValues("X-Message-Id", out var messageIds))
            result.MessageId = messageIds.FirstOrDefault() ?? string.Empty;
        
        if (sendGridResponse.IsSuccessStatusCode)
            return result;
        
        result.Errors.Add($"SendGrid returned status code {sendGridResponse.StatusCode}.");

        var messageBodyDictionary = await sendGridResponse.DeserializeResponseBodyAsync();
        if (messageBodyDictionary.TryGetValue("errors", out var errors))
            foreach (var error in errors)
                result.Errors.Add($"{error}");

        return result;
    }

    private static async Task<SendGridMessage> CreateSendGridMessageAsync(IMailEaseEmail email, CancellationToken cancellationToken)
    {
        var mailMessage = new SendGridMessage();
        mailMessage.SetSandBoxMode(email.Data.IsSandboxMode);
        
        mailMessage.SetFrom(email.Data.From.ToSendGridEmailAddress());
        
        mailMessage.AddTos(email.Data.To.Select(x => x.ToSendGridEmailAddress()).ToList());
        
        mailMessage.AddCcs(email.Data.Cc.Select(x => x.ToSendGridEmailAddress()).ToList());
        
        mailMessage.AddBccs(email.Data.Bcc.Select(x => x.ToSendGridEmailAddress()).ToList());
        
        // SendGrid only supports one reply-to address
        mailMessage.SetReplyTo(email.Data.ReplyTo.Select(x => x.ToSendGridEmailAddress()).First());
        
        mailMessage.SetSubject(email.Data.Subject);
        
        mailMessage.AddHeaders(email.Data.Headers.ToDictionary(x => x.Key, x => x.Value));
        
        mailMessage.AddCategories(email.Data.Tags.ToList());
        
        if (email.Data.Body.IsHtml)
            mailMessage.HtmlContent = email.Data.Body.Content;
        else
            mailMessage.PlainTextContent = email.Data.Body.Content;
        
        if (!string.IsNullOrWhiteSpace(email.Data.Body.PlainTextAlternativeBody))
            mailMessage.PlainTextContent = email.Data.Body.PlainTextAlternativeBody;
        
        switch (email.Data.Priority)
        {
            case EmailPriority.Normal:
                // This is the default used by SendGrid
                break;
            case EmailPriority.Low:
                // https://stackoverflow.com/questions/23230250/set-email-priority-with-sendgrid-api
                mailMessage.AddHeader("Priority", "Non-Urgent");
                mailMessage.AddHeader("Importance", "Low");
                // https://docs.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxcmail/2bb19f1b-b35e-4966-b1cb-1afd044e83ab
                mailMessage.AddHeader("X-Priority", "5");
                mailMessage.AddHeader("X-MSMail-Priority", "Low");
                break;
            case EmailPriority.High:
                // https://stackoverflow.com/questions/23230250/set-email-priority-with-sendgrid-api
                mailMessage.AddHeader("Priority", "Urgent");
                mailMessage.AddHeader("Importance", "High");
                // https://docs.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxcmail/2bb19f1b-b35e-4966-b1cb-1afd044e83ab
                mailMessage.AddHeader("X-Priority", "1");
                mailMessage.AddHeader("X-MSMail-Priority", "High");
                break;
        }
        
        mailMessage.AddAttachments(await Task.WhenAll(email.Data.Attachments.Select(x => ConvertEmailAttachmentToSendGridAttachmentAsync(x, cancellationToken))));
        
        return mailMessage;
    }
    
    private static async Task<Attachment> ConvertEmailAttachmentToSendGridAttachmentAsync(EmailAttachment attachment, CancellationToken cancellationToken) =>
        new()
        {
            Content = Convert.ToBase64String(await attachment.ToByteArrayAsync(cancellationToken)),
            Filename = attachment.FileName,
            Type = attachment.ContentType,
            ContentId = attachment.ContentId,
            Disposition = attachment.IsInline ? "inline" : "attachment"
        };
}