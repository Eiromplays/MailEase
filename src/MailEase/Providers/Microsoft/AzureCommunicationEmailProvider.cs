using System.Text.Json;
using MailEase.Exceptions;
using MailEase.Utils;

namespace MailEase.Providers.Microsoft;

public sealed class AzureCommunicationEmailProvider
    : BaseEmailProvider<AzureCommunicationEmailMessage>
{
    /// <summary>
    /// The maximum number of recipients allowed.
    /// The Azure Communication Services Email API supports up to 50 recipients per request.
    /// See: https://learn.microsoft.com/en-us/azure/communication-services/concepts/service-limits#email
    /// </summary>
    private const int MaxRecipients = 50;
    
    private readonly AzureCommunicationParams _azureCommunicationParams;

    public AzureCommunicationEmailProvider(AzureCommunicationParamsConnectionString azureCommunicationParams) 
        : this(
            ConnectionString.Parse(azureCommunicationParams.ConnectionString),
            azureCommunicationParams
        )
    {
    }
    
    private AzureCommunicationEmailProvider(
        ConnectionString connectionString,
        AzureCommunicationParams azureCommunicationParams
    )
        : base(
            new Uri(connectionString.GetRequired("endpoint")),
            new SharedKeyAuthHandler(connectionString.GetRequired("accessKey"))
        )
    {
        _azureCommunicationParams = azureCommunicationParams;
    }
    
    public AzureCommunicationEmailProvider(
        AzureCommunicationParamsEntraId azureCommunicationParams
    )
        : base(
            new Uri(azureCommunicationParams.Endpoint),
            new EntraIdAuthHandler(azureCommunicationParams.ClientSecretCredential)
        )
    {
        _azureCommunicationParams = azureCommunicationParams;
    }

    public override async Task<EmailResponse> SendEmailAsync(
        AzureCommunicationEmailMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ValidateEmailMessage(message); // Performs some common validations
        
        var responses = new List<AzureCommunicationEmailResponse>();

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
                
                var (response, error) = await PostJsonAsync<AzureCommunicationEmailResponse,
                    AzureCommunicationEmailErrorResponse>(
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
            var (response, error) = await PostJsonAsync<AzureCommunicationEmailResponse,
                AzureCommunicationEmailErrorResponse>(
                await MapToProviderRequestAsync(message)
            );

            if (error is not null)
                throw ConvertProviderErrorResponseToGenericError(error);
            
            if (response is not null)
                responses.Add(response);
        }

        return new EmailResponse(responses.Count > 0, responses.Select(response => response.Id).ToArray());
    }

    protected override MailEaseException ProviderSpecificValidation(
        AzureCommunicationEmailMessage request
    )
    {
        var mailEaseException = new MailEaseException();

        return mailEaseException;
    }

    private async Task<AzureCommunicationEmailRequest> MapToProviderRequestAsync(
        AzureCommunicationEmailMessage message
    )
    {
        var request = new AzureCommunicationEmailRequest
        {
            Headers = message.Headers,
            // Azure Communication Email doesn't support display names for the sender address.
            // You have to set the Display Name you want for the sender address in your azure portal.
            // https://learn.microsoft.com/en-us/rest/api/communication/dataplane/email/send?view=rest-communication-dataplane-2023-03-31&tabs=HTTP
            SenderAddress = message.From.Address,
            Content = new AzureCommunicationEmailContent
            {
                Subject = message.Subject,
                PlainText = message.Text ?? "",
                Html = message.Html ?? "",
            },
            Recipients = new AzureCommunicationEmailRecipients
            {
                To = message.ToAddresses
                    .Select(e => new AzureCommunicationEmailAddress(e.Address, e.Name ?? ""))
                    .ToList(),
                Cc = message.CcAddresses
                    .Select(e => new AzureCommunicationEmailAddress(e.Address, e.Name ?? ""))
                    .ToList(),
                Bcc = message.BccAddresses
                    .Select(e => new AzureCommunicationEmailAddress(e.Address, e.Name ?? ""))
                    .ToList()
            },
            ReplyTo = message.ReplyToAddresses
                .Select(e => new AzureCommunicationEmailAddress(e.Address, e.Name ?? ""))
                .ToList(),
            UserEngagementTrackingDisabled = message.UserEngagementTrackingDisabled
        };

        foreach (var attachment in message.Attachments)
        {
            request.Attachments.Add(
                new AzureCommunicationEmailAttachment
                {
                    ContentInBase64 = await StreamHelpers.StreamToBase64Async(attachment.Content),
                    ContentType = attachment.ContentType,
                    Name = attachment.FileName
                }
            );
        }

        return request;
    }

    private MailEaseException ConvertProviderErrorResponseToGenericError(
        AzureCommunicationEmailErrorResponse providerErrorResponse
    )
    {
        var genericError = new MailEaseException();
        genericError.AddError(
            new MailEaseErrorDetail(MailEaseErrorCode.Unknown, providerErrorResponse.Error.Message)
        );

        return genericError;
    }
}
