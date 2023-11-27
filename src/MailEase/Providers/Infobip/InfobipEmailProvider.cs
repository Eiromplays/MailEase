using System.Net.Http.Headers;
using System.Text.Json;
using MailEase.Exceptions;

namespace MailEase.Providers.Infobip;

public sealed class InfobipEmailProvider : BaseEmailProvider<InfobipMessage>
{
    public InfobipEmailProvider(InfobipParams infobipParams)
        : base(
            new Uri(infobipParams.BaseAddress, infobipParams.Path),
            new StaticAuthHandler(new AppToken(infobipParams.ApiKey))
        ) { }

    public override async Task SendEmailAsync(
        InfobipMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ValidateEmailMessage(message); // Performs some common validations

        var (data, error) = await PostMultiPartFormDataAsync<InfobipResponse, InfobipErrorResponse>(
            MapToProviderRequest(message)
        );

        if (error is not null)
            throw ConvertProviderErrorResponseToGenericError(error);

        Console.WriteLine(
            $"{JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })}"
        );
    }

    protected override MailEaseException ProviderSpecificValidation(InfobipMessage request)
    {
        var mailEaseException = new MailEaseException();

        if (request.SendAt.HasValue)
            if (request.SendAt.Value.UtcDateTime > DateTimeOffset.UtcNow.AddDays(30))
                mailEaseException.AddError(
                    BaseEmailMessageErrors.InvalidSendAt("SendAt must be in the past 30 days")
                );

        if (!string.IsNullOrWhiteSpace(request.AmpHtml) && string.IsNullOrWhiteSpace(request.Html))
            mailEaseException.AddError(
                BaseEmailMessageErrors.InvalidBody("Html cannot be empty when AmpHtml is provided")
            );

        return mailEaseException;
    }

    private MultipartFormDataContent MapToProviderRequest(InfobipMessage message)
    {
        var multipartFormDataContent = new MultipartFormDataContent
        {
            { new StringContent(message.From), "from" },
            { new StringContent(message.Subject), "subject" },
        };

        foreach (var toAddress in message.ToAddresses)
            multipartFormDataContent.Add(new StringContent(toAddress), "to");

        foreach (var ccAddress in message.CcAddresses)
            multipartFormDataContent.Add(new StringContent(ccAddress), "cc");

        foreach (var bccAddress in message.BccAddresses)
            multipartFormDataContent.Add(new StringContent(bccAddress), "bcc");

        if (!string.IsNullOrWhiteSpace(message.Text))
            multipartFormDataContent.Add(new StringContent(message.Text), "text");

        if (!string.IsNullOrWhiteSpace(message.Html))
            multipartFormDataContent.Add(new StringContent(message.Html), "html");

        foreach (var attachment in message.Attachments)
        {
            var attachmentStreamContent = new StreamContent(attachment.Content);
            if (MediaTypeHeaderValue.TryParse(attachment.ContentType, out var mediaTypeHeaderValue))
                attachmentStreamContent.Headers.ContentType = mediaTypeHeaderValue;

            multipartFormDataContent.Add(
                attachmentStreamContent,
                attachment.IsInline ? "inlineImage" : "attachment",
                attachment.FileName
            );
        }

        if (message.TemplateId.HasValue)
            multipartFormDataContent.Add(
                new StringContent(message.TemplateId.Value.ToString()),
                "templateId"
            );

        if (message.ReplyToAddresses.Count > 0)
            multipartFormDataContent.Add(
                new StringContent(
                    $"{message.ReplyToAddresses[0].Name} <{message.ReplyToAddresses[0].Address}>"
                ),
                "replyTo"
            );

        if (message.SendAt.HasValue)
            multipartFormDataContent.Add(
                new StringContent(
                    message.SendAt.Value.UtcDateTime.ToString("yyyy-MM-ddThh:mm:ss.fffZ")
                ),
                "sendAt"
            );

        if (!string.IsNullOrWhiteSpace(message.AmpHtml))
            multipartFormDataContent.Add(new StringContent(message.AmpHtml), "ampHtml");

        if (message.IntermediateReport.HasValue)
            multipartFormDataContent.Add(
                new StringContent(message.IntermediateReport.Value.ToString()),
                "intermediateReport"
            );

        if (!string.IsNullOrWhiteSpace(message.NotifyUrl))
            multipartFormDataContent.Add(new StringContent(message.NotifyUrl), "notifyUrl");

        if (!string.IsNullOrWhiteSpace(message.NotifyContentType))
            multipartFormDataContent.Add(
                new StringContent(message.NotifyContentType),
                "notifyContentType"
            );

        if (message.CallbackData is not null)
            multipartFormDataContent.Add(
                new StringContent(JsonSerializer.Serialize(message.CallbackData)),
                "callbackData"
            );

        if (message.Track.HasValue)
            multipartFormDataContent.Add(
                new StringContent(message.Track.Value.ToString()),
                "track"
            );

        if (message.TrackClicks.HasValue)
            multipartFormDataContent.Add(
                new StringContent(message.TrackClicks.Value.ToString()),
                "trackClicks"
            );

        if (message.TrackOpens.HasValue)
            multipartFormDataContent.Add(
                new StringContent(message.TrackOpens.Value.ToString()),
                "trackOpens"
            );

        if (!string.IsNullOrWhiteSpace(message.TrackingUrl))
            multipartFormDataContent.Add(new StringContent(message.TrackingUrl), "trackingUrl");

        if (!string.IsNullOrWhiteSpace(message.BulkId))
            multipartFormDataContent.Add(new StringContent(message.BulkId), "bulkId");

        if (!string.IsNullOrWhiteSpace(message.MessageId))
            multipartFormDataContent.Add(new StringContent(message.MessageId), "messageId");

        if (message.DefaultPlaceholders is not null)
            multipartFormDataContent.Add(
                new StringContent(JsonSerializer.Serialize(message.DefaultPlaceholders)),
                "defaultPlaceholders"
            );

        if (message.PreserveRecipients.HasValue)
            multipartFormDataContent.Add(
                new StringContent(message.PreserveRecipients.Value.ToString()),
                "preserveRecipients"
            );

        if (message.LandingPagePlaceholders is not null)
            multipartFormDataContent.Add(
                new StringContent(JsonSerializer.Serialize(message.LandingPagePlaceholders)),
                "landingPagePlaceholders"
            );

        if (!string.IsNullOrWhiteSpace(message.LandingPageId))
            multipartFormDataContent.Add(new StringContent(message.LandingPageId), "landingPageId");

        if (!string.IsNullOrWhiteSpace(message.TemplateLanguageVersion))
            multipartFormDataContent.Add(
                new StringContent(message.TemplateLanguageVersion),
                "templateLanguageVersion"
            );

        if (!string.IsNullOrWhiteSpace(message.ApplicationId))
            multipartFormDataContent.Add(new StringContent(message.ApplicationId), "applicationId");

        if (!string.IsNullOrWhiteSpace(message.EntityId))
            multipartFormDataContent.Add(new StringContent(message.EntityId), "entityId");

        return multipartFormDataContent;
    }

    private MailEaseException ConvertProviderErrorResponseToGenericError(
        InfobipErrorResponse providerErrorResponse
    )
    {
        var genericError = new MailEaseException();
        genericError.AddError(
            new MailEaseErrorDetail(
                MailEaseErrorCode.Unknown,
                providerErrorResponse.RequestError.ServiceException.Text
            )
        );
        foreach (
            var errorItem in providerErrorResponse.RequestError.ServiceException.ValidationErrors
        )
        {
            genericError.AddError(
                new MailEaseErrorDetail(
                    MailEaseErrorCode.Unknown,
                    $"{errorItem.Field}: {errorItem.Message}"
                )
            );
        }
        return genericError;
    }
}
