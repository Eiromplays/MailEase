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
            multipartFormDataContent.Add(
                new StringContent($"{toAddress.Name} <{toAddress.Address}>"),
                "to"
            );

        foreach (var ccAddress in message.CcAddresses)
            multipartFormDataContent.Add(
                new StringContent($"{ccAddress.Name} <{ccAddress.Address}>"),
                "cc"
            );

        foreach (var bccAddress in message.BccAddresses)
            multipartFormDataContent.Add(
                new StringContent($"{bccAddress.Name} <{bccAddress.Address}>"),
                "bcc"
            );

        multipartFormDataContent.Add(
            new StringContent(message.Body),
            message.IsHtmlBody ? "html" : "text"
        );

        if (message.Attachments.Count > 0)
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

        if (!string.IsNullOrWhiteSpace(message.TemplateId))
            multipartFormDataContent.Add(new StringContent(message.TemplateId), "templateId");

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

        return multipartFormDataContent;
    }

    private MailEaseException ConvertProviderErrorResponseToGenericError(
        InfobipErrorResponse providerErrorResponse
    )
    {
        var genericError = new MailEaseException();
        genericError.AddError(
            new MailEaseErrorDetail(MailEaseErrorCode.Unknown, providerErrorResponse.Text)
        );
        foreach (var errorItem in providerErrorResponse.ValidationErrors)
        {
            /*genericError.AddError(
                new MailEaseErrorDetail(errorItem.Message, MailEaseErrorCode.Unknown)
            );*/
        }
        return genericError;
    }
}
