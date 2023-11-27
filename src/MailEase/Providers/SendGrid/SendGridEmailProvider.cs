using System.Text.Json;
using MailEase.Exceptions;
using MailEase.Utils;

namespace MailEase.Providers.SendGrid;

public sealed class SendGridEmailProvider : BaseEmailProvider<SendGridMessage>
{
    public SendGridEmailProvider(SendGridParams sendGridParams)
        : base(
            new Uri($"{sendGridParams.BaseAddress}/{sendGridParams.Version}/{sendGridParams.Path}"),
            new StaticAuthHandler(new BearerToken(sendGridParams.ApiKey))
        ) { }

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
            CustomArgs = JsonSerializer.Serialize(message.CustomArgs),
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
