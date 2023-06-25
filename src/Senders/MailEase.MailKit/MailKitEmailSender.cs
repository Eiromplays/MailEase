// The code is based on the following PR: https://github.com/lukencode/FluentEmail/pull/324/commits/abbad109ed04af630c5e00a1293d2a075768f731
// And the original code from the same repo: https://github.com/lukencode/FluentEmail/blob/master/src/Senders/FluentEmail.MailKit/MailKitSender.cs

using System.Text;
using System.Text.RegularExpressions;
using MailEase.Extensions;
using MailEase.MailKit.Extensions;
using MailEase.Results;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

namespace MailEase.MailKit;

public partial class MailKitEmailSender : IEmailSender
{
    private readonly MailKitConfiguration _mailKitConfiguration;
    private readonly bool _isAmazonSes;

    public MailKitEmailSender(MailKitConfiguration mailKitConfiguration)
    {
        _mailKitConfiguration = mailKitConfiguration;
        _isAmazonSes = mailKitConfiguration.Server.Contains("amazonaws.com", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<SendEmailResult> SendAsync(IMailEaseEmail email, CancellationToken cancellationToken = default)
    {
        var mimeMessage = CreateMimeMessage(email, cancellationToken);
        var result = new SendEmailResult { MessageId = mimeMessage.MessageId };
        
        if (cancellationToken.IsCancellationRequested)
        {
            result.Errors.Add("The operation was cancelled.");
            return result;
        }

        try
        {
            if (_mailKitConfiguration.UsePickupDirectory)
            {
                var messageId = await SaveToPickupDirectoryAsync(mimeMessage, _mailKitConfiguration.MailPickupDirectory, cancellationToken);
                
                if (messageId is not null)
                    result.MessageId = messageId;
                return result;
            }
            
            using var smtpClient = _mailKitConfiguration.ProtocolLogger is null ? new SmtpClient() : new SmtpClient(_mailKitConfiguration.ProtocolLogger);
            
            smtpClient.CheckCertificateRevocation = _mailKitConfiguration.CheckCertificateRevocation;
            
            if (_mailKitConfiguration.ServerCertificateValidationCallback is not null)
                smtpClient.ServerCertificateValidationCallback = _mailKitConfiguration.ServerCertificateValidationCallback;
            
            if (_mailKitConfiguration.SecureSocketOptions.HasValue)
                await smtpClient.ConnectAsync(_mailKitConfiguration.Server, _mailKitConfiguration.Port,
                    _mailKitConfiguration.SecureSocketOptions.Value, cancellationToken);
            else
                await smtpClient.ConnectAsync(_mailKitConfiguration.Server, _mailKitConfiguration.Port, _mailKitConfiguration.UseSsl, cancellationToken);
            
            if (_mailKitConfiguration.RequiresAuthentication)
                await smtpClient.AuthenticateAsync(_mailKitConfiguration.User, _mailKitConfiguration.Password, cancellationToken);
            
            var manualResetEventSlim = new ManualResetEventSlim(false);
            if (_isAmazonSes)
            {
                // When using Amazon SES, we need to subscribe to the MessageSent event which will give us the overwritten MessageId.
                // Then we signal the ManualResetEventSlim.
                smtpClient.MessageSent += (sender, args) => SmtpClientOnMessageSent(sender, args, manualResetEventSlim);
            }
            else
                // When not using Amazon SES, we can signal the ManualResetEventSlim immediately.
                manualResetEventSlim.Set();
            
            await smtpClient.SendAsync(mimeMessage, cancellationToken);
            
            await smtpClient.DisconnectAsync(true, cancellationToken);
            
            // Wait for the ManualResetEventSlim to be signaled.
            manualResetEventSlim.Wait(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add(ex.Message);
        }
        
        return result;
    }

    private void SmtpClientOnMessageSent(object? sender, MessageSentEventArgs e, ManualResetEventSlim manualResetEventSlim)
    {
        // Example response format: "Ok 010701805bea386d-8411ef2a-5a8b-46bc-9cbb-585ace484c24-000000"
        var match = EmailRegex().Match(e.Response);
        if (match.Success)
        {
            // Strip "email-smtp" and similar prefixes from SMTP hostname: https://docs.aws.amazon.com/general/latest/gr/ses.html
            var domain = _mailKitConfiguration.Server[(_mailKitConfiguration.Server.IndexOf('.') + 1)..];
            var id = $"<{match.Groups[1].Value}@{domain}>";
            e.Message.MessageId = id;
        }

        // Trigger ManualResetEventSlim to signal that the processing is now complete
        manualResetEventSlim.Set();
    }

    /// <summary>
    /// Saves the <see cref="MimeMessage"/> to the pickup directory.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="pickupDirectory"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The message ID of the saved message if successful, otherwise <see langword="null"/>.</returns>
    private async Task<string?> SaveToPickupDirectoryAsync(MimeMessage message, string pickupDirectory,
        CancellationToken cancellationToken)
    {
        // Note: this will require that you know where the specified pickup directory is.
        var messageId = Guid.NewGuid().ToString();
        var path = Path.Combine(pickupDirectory, messageId + ".eml");

        if (File.Exists(path))
            return null;

        try
        {
            // We create the directory first in case it doesn't exist.
            Directory.CreateDirectory(pickupDirectory);

            await using var fileStream = new FileStream(path, FileMode.CreateNew);
            await message.WriteToAsync(fileStream, cancellationToken);

            return messageId;
        }
        catch (IOException)
        {
            // The file may have been created between our File.Exists() check and
            // our attempt to create the stream.
            throw;
        }
    }
    
    private static MimeMessage CreateMimeMessage(IMailEaseEmail email, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        
        if (!message.Headers.Contains(HeaderId.Subject))
            message.Headers.Add(HeaderId.Subject, Encoding.UTF8, email.Data.Subject);
        else
            message.Headers[HeaderId.Subject] = email.Data.Subject;
        
        message.Headers.Add(HeaderId.Encoding, Encoding.UTF8.EncodingName);
        
        message.From.Add(email.Data.From.ToMailboxAddress());

        var bodyBuilder = new BodyBuilder();
        if (!string.IsNullOrWhiteSpace(email.Data.Body.PlainTextAlternativeBody))
        {
            bodyBuilder.TextBody = email.Data.Body.PlainTextAlternativeBody;
            bodyBuilder.HtmlBody = email.Data.Body.Content;
        }
        else if (email.Data.Body.IsHtml)
        {
            bodyBuilder.HtmlBody = email.Data.Body.Content;
        }
        else
        {
            bodyBuilder.TextBody = email.Data.Body.Content;
        }
        
        email.Data.Attachments.ForEach(attachment =>
        {
            var contentType = ContentType.Parse(attachment.ContentType);
            
            if (attachment.IsInline)
            {
                var attachmentPart = bodyBuilder.LinkedResources.Add(attachment.FileName, attachment.Data,
                    contentType);
                attachmentPart.ContentId = string.IsNullOrWhiteSpace(attachment.ContentId)
                    ? attachment.FileName
                    : attachment.ContentId;
                
            }
            else
            {
                var attachmentPart = bodyBuilder.Attachments.Add(attachment.FileName, attachment.Data,
                    contentType, cancellationToken);
                attachmentPart.ContentId = attachment.ContentId;
            }
        });
        
        message.Body = bodyBuilder.ToMessageBody();
        
        message.Headers.AddRange(email.Data.Headers.Select(header => new Header(header.Key, header.Value)));
        
        message.To.AddRange(email.Data.To.Select(to => to.ToMailboxAddress()));
        message.Cc.AddRange(email.Data.Cc.Select(cc => cc.ToMailboxAddress()));
        message.Bcc.AddRange(email.Data.Bcc.Select(bcc => bcc.ToMailboxAddress()));
        message.ReplyTo.AddRange(email.Data.ReplyTo.Select(replyTo => replyTo.ToMailboxAddress()));

        message.Priority = email.Data.Priority switch
        {
            EmailPriority.Normal => MessagePriority.Normal,
            EmailPriority.Low => MessagePriority.NonUrgent,
            EmailPriority.High => MessagePriority.Urgent,
            _ => message.Priority
        };

        return message;
    }

    [GeneratedRegex("Ok ([0-9a-z\\-]+)")]
    private static partial Regex EmailRegex();
}