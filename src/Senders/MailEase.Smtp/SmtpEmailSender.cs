using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using MailEase.Extensions;
using MailEase.Results;
using MailEase.Smtp.Extensions;

namespace MailEase.Smtp;

public class SmtpEmailSender : IEmailSender
{
    private readonly Func<SmtpClient> _smtpClientFactory = null!;
    private readonly SmtpClient? _smtpClient;

    /// <summary>
    /// Creates a new instance of <see cref="SmtpEmailSender"/> using the default <see cref="SmtpClient"/> constructor and settings.
    /// </summary>
    public SmtpEmailSender() : this(() => new SmtpClient())
    {
    }
    
    /// <summary>
    /// Creates a new instance of <see cref="SmtpEmailSender"/> using the provided <see cref="SmtpClient"/> factory.
    /// </summary>
    /// <param name="smtpClientFactory">The factory to create <see cref="SmtpClient"/> instances.</param>
    public SmtpEmailSender(Func<SmtpClient> smtpClientFactory)
    {
        _smtpClientFactory = smtpClientFactory;
    }
    
    /// <summary>
    /// Creates a new instance of <see cref="SmtpEmailSender"/> using the provided <see cref="SmtpClient"/>.
    /// </summary>
    /// <param name="smtpClient">The <see cref="SmtpClient"/> to use.</param>
    public SmtpEmailSender(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
    }

    public async Task<SendEmailResult> SendAsync(IMailEaseEmail email, CancellationToken cancellationToken = default)
    {
        var result = new SendEmailResult();

        if (cancellationToken.IsCancellationRequested)
        {
            result.Errors.Add("The operation was cancelled.");
            return result;
        }
        
        var mailMessage = CreateMailMessage(email);

        var shouldDisposeSmtpClient = _smtpClient is null;
        try
        {
            var smtpClient = _smtpClient ?? _smtpClientFactory();

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add(ex.Message);
        }
        finally
        {
            if (shouldDisposeSmtpClient)
                _smtpClient?.Dispose();
        }

        return result;
    }
    
    private static MailMessage CreateMailMessage(IMailEaseEmail email)
    {
        MailMessage? mailMessage;

        // If the email has a plain text alternative, use that as the body and add the HTML as an alternate view.
        if (!string.IsNullOrWhiteSpace(email.Data.Body.PlainTextAlternativeBody))
        {
            mailMessage = new MailMessage
            {
                Subject = email.Data.Subject,
                Body = email.Data.Body.PlainTextAlternativeBody,
                IsBodyHtml = false,
                From = email.Data.From.ToMailAddress()
            };
            
            var mimeType = new ContentType("text/html; charset=UTF-8");
            var alternateView = AlternateView.CreateAlternateViewFromString(email.Data.Body.Content, mimeType);
            mailMessage.AlternateViews.Add(alternateView);
        }
        else
            mailMessage = new MailMessage
            {
                From = email.Data.From.ToMailAddress(),
                Subject = email.Data.Subject,
                Body = email.Data.Body.Content,
                IsBodyHtml = email.Data.Body.IsHtml,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
            };
        
        mailMessage.Priority = email.Data.Priority switch
        {
            EmailPriority.Normal => MailPriority.Normal,
            EmailPriority.Low => MailPriority.Low,
            EmailPriority.High => MailPriority.High,
            _ => mailMessage.Priority
        };

        mailMessage.To.AddRange(email.Data.To.Select(x => x.ToMailAddress()));

        mailMessage.CC.AddRange(email.Data.Cc.Select(x => x.ToMailAddress()));
        
        mailMessage.Bcc.AddRange(email.Data.Bcc.Select(x => x.ToMailAddress()));
        
        mailMessage.ReplyToList.AddRange(email.Data.ReplyTo.Select(x => x.ToMailAddress()));

        mailMessage.Attachments.AddRange(email.Data.Attachments.Select(x =>
        {
            var attachment = new Attachment(x.Data, x.FileName, x.ContentType)
            {
                ContentId = x.ContentId,
            };

            if (attachment.ContentDisposition is not null)
                attachment.ContentDisposition.Inline = x.IsInline;
            
            return attachment;
        }));
        
        email.Data.Headers.ForEach(x => mailMessage.Headers.Add(x.Key, x.Value));

        return mailMessage;
    }
}