using MailEase.Extensions;
using MailEase.Graph.Extensions;
using MailEase.Results;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace MailEase.Graph;

public class GraphEmailSender : IEmailSender
{
    private readonly GraphConfiguration _graphConfiguration;
    
    private readonly GraphServiceClient _graphServiceClient;

    public GraphEmailSender(GraphConfiguration graphConfiguration)
    {
        _graphConfiguration = graphConfiguration;

        _graphServiceClient = new GraphServiceClient(graphConfiguration.AuthenticationProvider);
    }

    public async Task<SendEmailResult> SendAsync(IMailEaseEmail email, CancellationToken cancellationToken = default)
    {
        var result = new SendEmailResult();
        
        var message = new Message
        {
            Subject = email.Data.Subject,
            Body = new ItemBody
            {
                Content = email.Data.Body.Content,
                ContentType = email.Data.Body.IsHtml ? BodyType.Html : BodyType.Text
            },
            From = email.Data.From.ToGraphRecipient()
        };
        
        (message.ToRecipients ?? new List<Recipient>()).AddRange(email.Data.To.Select(to => to.ToGraphRecipient()));
        (message.CcRecipients ?? new List<Recipient>()).AddRange(email.Data.Cc.Select(cc => cc.ToGraphRecipient()));
        (message.BccRecipients ?? new List<Recipient>()).AddRange(email.Data.Bcc.Select(bcc => bcc.ToGraphRecipient()));
        (message.ReplyTo ?? new List<Recipient>()).AddRange(email.Data.ReplyTo.Select(replyTo => replyTo.ToGraphRecipient()));

        foreach (var attachment in email.Data.Attachments)
        {
            (message.Attachments ?? new List<Attachment>()).Add(new FileAttachment
            {
                Name = attachment.FileName,
                ContentType = attachment.ContentType,
                ContentBytes = await attachment.ToByteArrayAsync(cancellationToken),
                ContentId = attachment.ContentId,
                IsInline = attachment.IsInline
            });
        }

        message.Importance = email.Data.Priority switch
        {
            EmailPriority.Normal => Importance.Normal,
            EmailPriority.Low => Importance.Low,
            EmailPriority.High => Importance.High,
            _ => Importance.Normal
        };

        try
        {
            var sendMailPostRequestBody = new SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = _graphConfiguration.SaveSentEmails
            };

            await _graphServiceClient.Users[email.Data.From.Address]
                .SendMail
                .PostAsync(sendMailPostRequestBody, cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(message.Id))
                result.MessageId = message.Id;
        }
        catch (Exception ex)
        {
            result.Errors.Add(ex.Message);
        }
        
        return result;
    }
}