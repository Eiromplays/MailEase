using System.Text.Json;
using MailEase.Exceptions;
using MailEase.Utils;

namespace MailEase.Providers.Microsoft;

public sealed class AzureCommunicationEmailProvider
    : BaseEmailProvider<AzureCommunicationEmailMessage>
{
    private readonly AzureCommunicationParams _azureCommunicationParams;

    public AzureCommunicationEmailProvider(AzureCommunicationParams azureCommunicationParams)
        : this(
            ConnectionString.Parse(azureCommunicationParams.ConnectionString),
            azureCommunicationParams
        ) { }

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

    public override async Task SendEmailAsync(
        AzureCommunicationEmailMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ValidateEmailMessage(message); // Performs some common validations

        var (data, error) = await PostJsonAsync<
            AzureCommunicationEmailResponse,
            AzureCommunicationEmailErrorResponse
        >(
            $"/emails:send?api-version={_azureCommunicationParams.ApiVersion}",
            await MapToProviderRequestAsync(message)
        );

        if (error is not null)
            throw ConvertProviderErrorResponseToGenericError(error);

        Console.WriteLine(
            $"{JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })}"
        );
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
