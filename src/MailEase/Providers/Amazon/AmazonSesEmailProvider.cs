using System.Text;
using MailEase.Exceptions;
using MimeKit;

namespace MailEase.Providers.Amazon;

public sealed class AmazonSesEmailProvider : BaseEmailProvider<AmazonSesMessage>
{
    /// <summary>
    /// The maximum number of recipients allowed.
    /// The Amazon SES API supports up to 50 recipients per request.
    /// See: https://docs.aws.amazon.com/ses/latest/dg/quotas.html
    /// </summary>
    private const int MaxRecipients = 50;
    
    public AmazonSesEmailProvider(AmazonSesParams amazonSesParams)
        : base(
            new Uri(
                new Uri($"https://email.{amazonSesParams.Region}.amazonaws.com"),
                amazonSesParams.Version + amazonSesParams.Path
            ),
            new SesAuthHandler(
                amazonSesParams.AccessKeyId,
                amazonSesParams.SecretAccessKey,
                amazonSesParams.SessionToken,
                amazonSesParams.Region
            )
        ) { }

    public override async Task<EmailResponse> SendEmailAsync(
        AmazonSesMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ValidateEmailMessage(message); // Performs some common validations

        var responses = new List<AmazonSesResponse>();

        var totalRecipients = message.ToAddresses.Count + message.CcAddresses.Count + message.BccAddresses.Count;
        var recipientsExceedsLimit = totalRecipients > MaxRecipients;
        
        if (recipientsExceedsLimit && message.UseSplitting)
        {
            const int chunkSize = MaxRecipients / 3;
            
            var toAddressChunks = message.ToAddresses.Chunk(chunkSize).ToList();
            
            var ccAddressChunks = message.CcAddresses.Chunk(chunkSize).ToList();
            
            var bccAddressChunks = message.BccAddresses.Chunk(chunkSize).ToList();

            var maxChunks = Math.Max(toAddressChunks.Count, Math.Max(ccAddressChunks.Count, bccAddressChunks.Count));
            for (var i = 0; i < maxChunks; i++)
            {
                message.ToAddresses.Clear();
                message.ToAddresses.AddRange(toAddressChunks.ElementAtOrDefault(i)?.ToList() ?? []);
                message.CcAddresses.Clear();
                message.CcAddresses.AddRange(ccAddressChunks.ElementAtOrDefault(i)?.ToList() ?? []);
                message.BccAddresses.Clear();
                message.BccAddresses.AddRange(bccAddressChunks.ElementAtOrDefault(i)?.ToList() ?? []);
                
                var (response, error) = await PostJsonAsync<AmazonSesResponse, AmazonSesErrorResponse>(
                    await MapToProviderRequestAsync(message)
                );

                if (error is not null)
                    throw ConvertProviderErrorResponseToGenericError(error);
                
                if (response is not null) 
                    responses.Add(response);
            }
        }
        else
        {
            var (response, error) = await PostJsonAsync<AmazonSesResponse, AmazonSesErrorResponse>(
                await MapToProviderRequestAsync(message)
            );

            if (error is not null)
                throw ConvertProviderErrorResponseToGenericError(error);
            
            if (response is not null)
                responses.Add(response);
        }

        return new EmailResponse(responses.Count > 0, responses.Select(response => response.MessageId).ToArray());
    }
    
    protected override MailEaseException ProviderSpecificValidation(AmazonSesMessage request)
    {
        var mailEaseException = new MailEaseException();

        // Throw an exception if the total number of recipients exceeds the limit and useSplitting is disabled.
        if (request.ToAddresses.Count + request.CcAddresses.Count + request.BccAddresses.Count > MaxRecipients && !request.UseSplitting)
            mailEaseException.AddError(
                BaseEmailMessageErrors.RecipientsExceedLimit(MaxRecipients)
            );

        return mailEaseException;
    }

    private async Task<AmazonSesRequest> MapToProviderRequestAsync(AmazonSesMessage message)
    {
        var mimeMessage = new MimeMessage();

        if (!mimeMessage.Headers.Contains(HeaderId.Subject))
            mimeMessage.Headers.Add(HeaderId.Subject, Encoding.UTF8, message.Subject);
        else
            mimeMessage.Headers[HeaderId.Subject] = message.Subject;

        mimeMessage.Headers.Add(HeaderId.Encoding, Encoding.UTF8.EncodingName);

        mimeMessage.From.Add(message.From);

        var bodyBuilder = new BodyBuilder();
        if (!string.IsNullOrWhiteSpace(message.Text))
            bodyBuilder.TextBody = message.Text;
        if (!string.IsNullOrWhiteSpace(message.Html))
            bodyBuilder.HtmlBody = message.Html;

        foreach (var attachment in message.Attachments)
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
            message.ToAddresses.Select(e => new MailboxAddress(e.Name, e.Address))
        );

        mimeMessage.Cc.AddRange(
            message.CcAddresses.Select(e => new MailboxAddress(e.Name, e.Address))
        );

        mimeMessage.Bcc.AddRange(
            message.BccAddresses.Select(e => new MailboxAddress(e.Name, e.Address))
        );

        mimeMessage.ReplyTo.AddRange(
            message.ReplyToAddresses.Select(e => new MailboxAddress(e.Name, e.Address))
        );

        foreach (var header in message.Headers)
        {
            mimeMessage.Headers.Add(header.Key, header.Value);
        }

        using var memoryStream = new MemoryStream();
        await mimeMessage.WriteToAsync(memoryStream);
        memoryStream.Position = 0;
        var mimeMessageBytes = memoryStream.ToArray();

        var content = new AmazonSesRequestContent
        {
            Raw = new AmazonSesRequestContentRaw
            {
                Data = Convert.ToBase64String(mimeMessageBytes),
            },
            Template = message.Template,
        };

        var request = new AmazonSesRequest
        {
            ConfigurationSetName = message.ConfigurationSetName,
            Content = content,
            Destination = new AmazonSesRequestDestination
            {
                BccAddresses = message.BccAddresses.Select(e => e.ToString()).ToList(),
                CcAddresses = message.CcAddresses.Select(e => e.ToString()).ToList(),
                ToAddresses = message.ToAddresses.Select(e => e.ToString()).ToList(),
            },
            EmailTags = message.EmailTags
                .Select(x => new AmazonSesEmailTag { Name = x.Key, Value = x.Value })
                .ToList(),
            FeedbackForwardingEmailAddress = message.FeedbackForwardingEmailAddress,
            FeedbackForwardingEmailAddressIdentityArn =
                message.FeedbackForwardingEmailAddressIdentityArn,
            FromEmailAddress = message.From,
            FromEmailAddressIdentityArn = message.FromEmailAddressIdentityArn,
            ListManagementOptions = message.ListManagementOptions,
            ReplyToAddresses = message.ReplyToAddresses.Select(e => e.ToString()).ToList(),
        };

        return request;
    }

    private MailEaseException ConvertProviderErrorResponseToGenericError(
        AmazonSesErrorResponse providerErrorResponse
    )
    {
        var genericError = new MailEaseException();
        genericError.AddError(
            new MailEaseErrorDetail(MailEaseErrorCode.Unknown, providerErrorResponse.Message)
        );
        return genericError;
    }
}
