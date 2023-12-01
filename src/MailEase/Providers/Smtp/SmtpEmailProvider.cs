using System.Text;
using MailEase.Exceptions;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

namespace MailEase.Providers.Smtp;

public sealed class SmtpEmailProvider : BaseEmailProvider<SmtpMessage>
{
    private readonly SmtpParams _smtpParams;

    public SmtpEmailProvider(SmtpParams smtpParams)
    {
        if (string.IsNullOrWhiteSpace(smtpParams.Host))
            throw new InvalidOperationException("Host cannot be empty.");

        if (string.IsNullOrWhiteSpace(smtpParams.UserName))
            throw new InvalidOperationException("Username cannot be empty.");

        if (string.IsNullOrWhiteSpace(smtpParams.Password))
            throw new InvalidOperationException("Password cannot be empty.");

        _smtpParams = smtpParams;
    }

    public override async Task<EmailResponse> SendEmailAsync(
        SmtpMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ValidateEmailMessage(message); // Performs some common validations

        try
        {
            using var client = new SmtpClient();
            if (_smtpParams.SecureSocketOptions.HasValue)
                await client.ConnectAsync(
                    _smtpParams.Host,
                    _smtpParams.Port,
                    _smtpParams.SecureSocketOptions.Value,
                    cancellationToken
                );
            else
                await client.ConnectAsync(
                    _smtpParams.Host,
                    _smtpParams.Port,
                    _smtpParams.UseSsl,
                    cancellationToken
                );

            // This block performs SMTP client authentication when required by the server.
            // It's generally necessary as most SMTP servers require authentication today.
            if (_smtpParams.RequiresAuthentication)
                await client.AuthenticateAsync(
                    _smtpParams.UserName,
                    _smtpParams.Password,
                    cancellationToken
                );

            await client.SendAsync(await MapToProviderRequestAsync(message), cancellationToken);

            await client.DisconnectAsync(true, cancellationToken);

            return new EmailResponse(true);
        }
        catch (Exception ex)
        {
            throw ConvertProviderErrorResponseToGenericError(ex);
        }
    }

    private async Task<MimeMessage> MapToProviderRequestAsync(SmtpMessage smtpMessage)
    {
        var mimeMessage = new MimeMessage();

        if (!mimeMessage.Headers.Contains(HeaderId.Subject))
            mimeMessage.Headers.Add(HeaderId.Subject, Encoding.UTF8, smtpMessage.Subject);
        else
            mimeMessage.Headers[HeaderId.Subject] = smtpMessage.Subject;

        mimeMessage.Headers.Add(HeaderId.Encoding, Encoding.UTF8.EncodingName);

        mimeMessage.From.Add(smtpMessage.From);

        var bodyBuilder = new BodyBuilder();
        if (!string.IsNullOrWhiteSpace(smtpMessage.Text))
            bodyBuilder.TextBody = smtpMessage.Text;
        if (!string.IsNullOrWhiteSpace(smtpMessage.Html))
            bodyBuilder.HtmlBody = smtpMessage.Html;

        foreach (var attachment in smtpMessage.Attachments)
        {
            var attachmentPart = await bodyBuilder.Attachments.AddAsync(
                attachment.FileName,
                attachment.Content,
                ContentType.Parse(attachment.ContentType)
            );

            if (!string.IsNullOrWhiteSpace(attachment.ContentId))
                attachmentPart.ContentId = attachment.ContentId;
        }

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        mimeMessage.To.AddRange(
            smtpMessage.ToAddresses.Select(e => new MailboxAddress(e.Name, e.Address))
        );

        mimeMessage.Cc.AddRange(
            smtpMessage.CcAddresses.Select(e => new MailboxAddress(e.Name, e.Address))
        );

        mimeMessage.Bcc.AddRange(
            smtpMessage.BccAddresses.Select(e => new MailboxAddress(e.Name, e.Address))
        );

        mimeMessage.ReplyTo.AddRange(
            smtpMessage.ReplyToAddresses.Select(e => new MailboxAddress(e.Name, e.Address))
        );

        foreach (var header in smtpMessage.Headers)
        {
            mimeMessage.Headers.Add(header.Key, header.Value);
        }

        return mimeMessage;
    }

    private MailEaseException ConvertProviderErrorResponseToGenericError(Exception exception)
    {
        var mailEaseException = new MailEaseException();

        switch (exception)
        {
            case ServiceNotConnectedException serviceNotConnectedException:
                mailEaseException.AddError(
                    new MailEaseErrorDetail(
                        MailEaseErrorCode.Unknown,
                        serviceNotConnectedException.Message
                    )
                );
                break;
            case ServiceNotAuthenticatedException serviceNotAuthenticatedException:
                mailEaseException.AddError(
                    new MailEaseErrorDetail(
                        MailEaseErrorCode.Unknown,
                        serviceNotAuthenticatedException.Message
                    )
                );
                break;
            default:
                mailEaseException.AddError(
                    new MailEaseErrorDetail(MailEaseErrorCode.Unknown, exception.Message)
                );
                break;
        }

        return mailEaseException;
    }
}
