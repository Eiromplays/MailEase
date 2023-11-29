using MailEase.Exceptions;
using MailEase.Utils;

namespace MailEase.Providers.Mailtrap;

public sealed class MailtrapEmailProvider : BaseEmailProvider<MailtrapMessage>
{
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

        var (response, error) = await PostJsonAsync<MailtrapResponse, MailtrapErrorResponse>(
            await MapToProviderRequestAsync(message)
        );

        if (error is not null)
            throw ConvertProviderErrorResponseToGenericError(error);

        return new EmailResponse(response is not null, response?.MessageIds.ToArray());
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
