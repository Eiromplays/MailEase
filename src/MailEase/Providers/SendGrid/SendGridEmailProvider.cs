using MailEase.Exceptions;
using MailEase.Utils;

namespace MailEase.Providers.SendGrid;

public sealed class SendGridEmailProvider : BaseEmailProvider<SendGridMessage>
{
    /// <summary>
    /// The maximum number of recipients allowed.
    /// The SendGrid API supports up to 1000 recipients per request.
    /// See: https://docs.sendgrid.com/api-reference/mail-send/limitations
    /// </summary>
    private const int MaxRecipients = 1000;
    
    public SendGridEmailProvider(SendGridParams sendGridParams)
        : base(
            new Uri($"{sendGridParams.BaseAddress}/{sendGridParams.Version}/{sendGridParams.Path}"),
            new StaticAuthHandler(new BearerToken(sendGridParams.ApiKey))
        ) { }

    public override async Task<EmailResponse> SendEmailAsync(
        SendGridMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ValidateEmailMessage(message); // Performs some common validations
        
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
                var (_, error) = await PostJsonAsync<object, SendGridErrorResponse>(
                    await MapToProviderRequestAsync(message)
                );
                
                if (error is not null)
                    throw ConvertProviderErrorResponseToGenericError(error);
            }
        }
        else
        {
            var (_, error) = await PostJsonAsync<object, SendGridErrorResponse>(
                await MapToProviderRequestAsync(message)
            );

            if (error is not null)
                throw ConvertProviderErrorResponseToGenericError(error);
        }

        return new EmailResponse(true);
    }

    protected override MailEaseException ProviderSpecificValidation(SendGridMessage request)
    {
        var mailEaseException = new MailEaseException();

        if (request.SendAt.HasValue)
            if (request.SendAt.Value.UtcDateTime > DateTimeOffset.UtcNow.AddHours(72))
                mailEaseException.AddError(
                    BaseEmailMessageErrors.InvalidSendAt("SendAt must be in the past 72 hours")
                );
        
        // Throw an exception if the total number of recipients exceeds the limit and useSplitting is disabled.
        if (request.ToAddresses.Count + request.CcAddresses.Count + request.BccAddresses.Count > MaxRecipients && !request.UseSplitting)
            mailEaseException.AddError(
                BaseEmailMessageErrors.RecipientsExceedLimit(MaxRecipients)
            );

        return mailEaseException;
    }

    private async Task<SendGridRequest> MapToProviderRequestAsync(SendGridMessage message)
    {
        var content = new List<SendGridContent>();
        if (!string.IsNullOrWhiteSpace(message.Text))
            content.Add(new SendGridContent { Type = "text/plain", Value = message.Text });

        if (!string.IsNullOrWhiteSpace(message.Html))
            content.Add(new SendGridContent { Type = "text/html", Value = message.Html });

        var request = new SendGridRequest
        {
            From = message.From,
            Personalizations =
            [
                new SendGridPersonalization
                {
                    To = message.ToAddresses
                        .Select(e => new SendGridEmailAddress(e.Address, e.Name))
                        .ToList(),
                    Cc =
                        message.CcAddresses.Count > 0
                            ? message.CcAddresses
                                .Select(e => new SendGridEmailAddress(e.Address, e.Name))
                                .ToList()
                            : null,
                    Bcc =
                        message.BccAddresses.Count > 0
                            ? message.BccAddresses
                                .Select(e => new SendGridEmailAddress(e.Address, e.Name))
                                .ToList()
                            : null,
                }
            ],
            ReplyToList = message.ReplyToAddresses
                .Select(e => new SendGridEmailAddress(e.Address, e.Name))
                .ToList(),
            Subject = message.Subject,
            Content = content,
            TemplateId = message.TemplateId,
            Headers = message.Headers,
            Categories = message.Categories,
            CustomArgs = message.CustomArgs,
            SendAt = message.SendAt?.ToUnixTimeSeconds(),
            BatchId = message.BatchId,
            Asm = message.Asm,
            IpPoolName = message.IpPoolName,
            MailSettings = message.MailSettings,
            TrackingSettings = message.TrackingSettings
        };

        if (message.Attachments.Count > 0)
            request.Attachments = new List<SendGridAttachment>();

        foreach (var attachment in message.Attachments)
        {
            request.Attachments!.Add(
                new SendGridAttachment
                {
                    // Base 64 encoded content
                    Content = await StreamHelpers.StreamToBase64Async(attachment.Content),
                    Type = attachment.ContentType,
                    Filename = attachment.FileName,
                    Disposition = attachment.IsInline ? "inline" : "attachment",
                    ContentId = attachment.ContentId
                }
            );
        }

        return request;
    }

    private MailEaseException ConvertProviderErrorResponseToGenericError(
        SendGridErrorResponse providerErrorResponse
    )
    {
        var genericError = new MailEaseException();
        foreach (var errorItem in providerErrorResponse.Errors)
        {
            genericError.AddError(
                new MailEaseErrorDetail(MailEaseErrorCode.Unknown, errorItem.Message)
            );
        }
        return genericError;
    }
}
