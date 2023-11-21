using System.Net.Http.Headers;
using MailEase.Exceptions;

namespace MailEase.Providers.Mailgun;

public sealed class MailgunEmailProvider : BaseEmailProvider<MailgunMessage>
{
    public MailgunEmailProvider(MailgunParams mailgunParams)
        : base(
            new Uri(new Uri(mailgunParams.BaseAddress), mailgunParams.Path),
            new StaticAuthHandler(new BearerToken(mailgunParams.ApiKey))
        ) { }

    public override async Task SendEmailAsync(
        MailgunMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ValidateEmailMessage(message); // Performs some common validations

        var (_, error) = await PostJsonAsync<MailgunResponse, MailgunErrorResponse>(
            MapToProviderRequest(message)
        );

        if (error is not null)
            throw ConvertProviderErrorResponseToGenericError(error);
    }

    private MultipartFormDataContent MapToProviderRequest(MailgunMessage message)
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
        if (!string.IsNullOrWhiteSpace(message.PlainTextBody))
            multipartFormDataContent.Add(new StringContent(message.PlainTextBody), "text");

        foreach (var attachment in message.Attachments)
        {
            var attachmentStreamContent = new StreamContent(attachment.Content);
            if (MediaTypeHeaderValue.TryParse(attachment.ContentType, out var mediaTypeHeaderValue))
                attachmentStreamContent.Headers.ContentType = mediaTypeHeaderValue;

            multipartFormDataContent.Add(
                attachmentStreamContent,
                attachment.IsInline ? "inline" : "attachment",
                attachment.FileName
            );
        }

        if (!string.IsNullOrWhiteSpace(message.Template))
            multipartFormDataContent.Add(new StringContent(message.Template), "template");

        foreach (var header in message.Headers)
        {
            var key = header.Key.StartsWith("h:") ? header.Key : $"h:{header.Key}";

            multipartFormDataContent.Add(new StringContent(header.Value), key);
        }

        return multipartFormDataContent;
    }

    private MailEaseException ConvertProviderErrorResponseToGenericError(
        MailgunErrorResponse providerErrorResponse
    )
    {
        var genericError = new MailEaseException();
        /*foreach (var errorItem in providerErrorResponse.Errors)
        {
            genericError.AddError(new MailEaseErrorDetail(MailEaseErrorCode.Unknown, errorItem));
        }*/
        return genericError;
    }
}
