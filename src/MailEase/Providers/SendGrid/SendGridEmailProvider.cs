using MailEase.Exceptions;
using MailEase.Helpers;

namespace MailEase.Providers.SendGrid;

public sealed class SendGridEmailProvider : BaseEmailProvider<SendGridMessage>
{
    private readonly SendGridParams _sendGridParams;

    public SendGridEmailProvider(SendGridParams sendGridParams)
        : base(
            new Uri($"{sendGridParams.BaseAddress}/{sendGridParams.Version}/{sendGridParams.Path}"),
            new StaticAuthHandler(new BearerToken(sendGridParams.ApiKey))
        )
    {
        _sendGridParams = sendGridParams;
    }

    public override async Task SendEmailAsync(
        SendGridMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ValidateEmailMessage(message); // Performs some common validations

        var (_, error) = await PostJsonAsync<object, SendGridErrorResponse>(
            await MapToProviderRequestAsync(message)
        );

        if (error is not null)
            throw ConvertProviderErrorResponseToGenericError(error);
    }

    protected override MailEaseException ProviderSpecificValidation(SendGridMessage request)
    {
        var mailEaseException = new MailEaseException();

        if (request.SendAt.HasValue)
            if (request.SendAt.Value.UtcDateTime > DateTimeOffset.UtcNow.AddHours(72))
                mailEaseException.AddError(
                    BaseEmailMessageErrors.InvalidSendAt("SendAt must be in the past 72 hours")
                );

        return mailEaseException;
    }

    private async Task<SendGridRequest> MapToProviderRequestAsync(SendGridMessage message)
    {
        var content = new List<SendGridContent>();
        if (message.IsHtmlBody)
        {
            content.Add(new SendGridContent { Type = "text/html", Value = message.Body });

            if (!string.IsNullOrWhiteSpace(message.PlainTextBody))
                content.Add(
                    new SendGridContent { Type = "text/plain", Value = message.PlainTextBody }
                );
        }
        else
            content.Add(new SendGridContent { Type = "text/plain", Value = message.Body });

        var request = new SendGridRequest
        {
            From = message.From,
            Personalizations = new List<SendGridPersonalization>
            {
                new()
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
            },
            ReplyToList = message.ReplyToAddresses
                .Select(e => new SendGridEmailAddress(e.Address, e.Name))
                .ToList(),
            Subject = message.Subject,
            Content = content,
            Headers = message.Headers,
            MailSettings = new SendGridMailSettings
            {
                SandBoxMode = new SendGridSandBoxMode { Enable = message.SandBoxMode }
            },
            TemplateId = message.TemplateId,
            SendAt = message.SendAt?.ToUnixTimeSeconds()
        };

        foreach (var attachment in message.Attachments)
        {
            (request.Attachments ?? new List<SendGridAttachment>()).Add(
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
