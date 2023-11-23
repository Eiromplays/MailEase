namespace MailEase.Providers.Amazon;

public sealed record AmazonSesParams
{
    /// <summary>
    /// Gets the access key ID associated with the property.
    /// </summary>
    /// <value>
    /// The access key ID.
    /// </value>
    public string AccessKeyId { get; }

    /// <summary>
    /// Gets the secret access key used for authentication.
    /// </summary>
    /// <value>
    /// The secret access key.
    /// </value>
    public string SecretAccessKey { get; }

    /// <summary>
    /// Gets the region of the object.
    /// </summary>
    /// <value>
    /// The region.
    /// </value>
    public string Region { get; }

    /// <summary>
    /// Gets the version of the software.
    /// </summary>
    /// <value>
    /// The version.
    /// </value>
    public string Version { get; }

    /// <summary>
    /// Gets the path of the property Path.
    /// </summary>
    /// <value>
    /// The path of the property.
    /// </value>
    public string Path { get; }

    public AmazonSesParams(
        string accessKeyId,
        string secretAccessKey,
        string region,
        string version = "/v2",
        string path = "/email/outbound-emails"
    )
    {
        if (string.IsNullOrWhiteSpace(accessKeyId))
            throw new ArgumentNullException(nameof(accessKeyId), "Access key ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(secretAccessKey))
            throw new ArgumentNullException(
                nameof(secretAccessKey),
                "Secret access key cannot be empty."
            );

        if (string.IsNullOrWhiteSpace(region))
            throw new ArgumentNullException(nameof(region), "Region cannot be empty.");

        AccessKeyId = accessKeyId;
        SecretAccessKey = secretAccessKey;
        Region = region;
        Version = version;
        Path = path;
    }
}
