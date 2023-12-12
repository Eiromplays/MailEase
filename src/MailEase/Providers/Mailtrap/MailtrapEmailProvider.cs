using MailEase.Exceptions;
using MailEase.Utils;

namespace MailEase.Providers.Mailtrap;

public sealed class MailtrapEmailProvider : BaseEmailProvider<MailtrapMessage>
{
    /// <summary>
    /// The maximum number of recipients allowed.
    /// The Mailtrap API supports up to 1000 recipients per request.
    /// See: https://api-docs.mailtrap.io/docs/mailtrap-api-docs/67f1d70aeb62c-send-email
    /// </summary>
    private const int MaxRecipients = 1000;

    public MailtrapEmailProvider(MailtrapParams mailtrapParams)
        : base(
            new Uri(new Uri(mailtrapParams.BaseAddress), mailtrapParams.Path),
            new StaticAuthHandler(new BearerToken(mailtrapParams.ApiKey))
        ) { }

    public override async Task<EmailResponse> SendEmailAsync(
        MailtrapMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ValidateEmailMessage(message); // Performs some common validations

        var responses = new List<MailtrapResponse>();

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
                var (response, error) = await PostJsonAsync<MailtrapResponse, MailtrapErrorResponse>(
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
            var (response, error) = await PostJsonAsync<MailtrapResponse, MailtrapErrorResponse>(
                await MapToProviderRequestAsync(message)
            );

            if (error is not null)
                throw ConvertProviderErrorResponseToGenericError(error);
            
            if (response is not null)
                responses.Add(response);
        }

        return new EmailResponse(responses.Count > 0, responses.SelectMany(response => response.MessageIds).ToArray());
    }

    protected override MailEaseException ProviderSpecificValidation(MailtrapMessage request)
    {
        var mailEaseException = new MailEaseException();

        // Throw an exception if the total number of recipients exceeds the limit and useSplitting is disabled.
        if (request.ToAddresses.Count + request.CcAddresses.Count + request.BccAddresses.Count > MaxRecipients && !request.UseSplitting)
            mailEaseException.AddError(
                BaseEmailMessageErrors.RecipientsExceedLimit(MaxRecipients)
            );

        return mailEaseException;
    }

    private async Task<MailtrapRequest> MapToProviderRequestAsync(MailtrapMessage message)
    {
        var request = new MailtrapRequest
        {
            From = message.From,
            To = message.ToAddresses
                .Select(e => new MailtrapEmailAddress(e.Address, e.Name))
                .ToList(),
            Cc = message.CcAddresses
                .Select(e => new MailtrapEmailAddress(e.Address, e.Name))
                .ToList(),
            Bcc = message.BccAddresses
                .Select(e => new MailtrapEmailAddress(e.Address, e.Name))
                .ToList(),
            Headers = message.Headers,
            CustomVariables = message.CustomVariables,
            Subject = message.Subject,
            Text = message.Text,
            Html = message.Html,
            Category = message.Category,
        };

        foreach (var attachment in message.Attachments)
        {
            (request.Attachments ?? new List<MailtrapAttachment>()).Add(
                new MailtrapAttachment
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
        MailtrapErrorResponse providerErrorResponse
    )
    {
        var genericError = new MailEaseException();
        foreach (var errorItem in providerErrorResponse.Errors)
        {
            genericError.AddError(new MailEaseErrorDetail(MailEaseErrorCode.Unknown, errorItem));
        }
        return genericError;
    }
}
