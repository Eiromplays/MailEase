/*
 This file contains code derived from Stowage (https://github.com/aloneguid/stowage/blob/3b83e2af3925def45763a6ca052ae3f54a65cd55/src/Stowage/Impl/Amazon/CredentialFileParser.cs),
 under the Apache 2.0 license. See the 'licenses' directory for full license details.
*/

namespace MailEase.Providers.Amazon;

/// <summary>
/// Parses credential file from ~/.aws/credentials.
/// File structure is described here: https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-files.html#cli-configure-files-format-profile
/// </summary>
internal sealed class CredentialFileParser
{
    public const string DefaultProfileName = "default";

    private readonly StructuredIniFile? _credIniFile;
    private readonly StructuredIniFile? _configIniFile;
    private const string AccessKeyIdKeyName = "aws_access_key_id";
    private const string SecretAccessKeyKeyName = "aws_secret_access_key";
    private const string SessionTokenKeyName = "aws_session_token";

    public CredentialFileParser()
    {
        var credFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".aws",
            "credentials"
        );
        var configFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".aws",
            "config"
        );
        var exists = File.Exists(credFilePath);
        _credIniFile = exists ? StructuredIniFile.FromString(File.ReadAllText(credFilePath)) : null;
        _configIniFile = File.Exists(configFilePath)
            ? StructuredIniFile.FromString(File.ReadAllText(configFilePath))
            : null;
    }

    public string[]? ProfileNames => _credIniFile?.SectionNames;

    /// <summary>
    /// Fills profile configuration from config files.
    /// </summary>
    /// <param name="profileName"></param>
    /// <param name="accessKeyId"></param>
    /// <param name="secretAccessKey"></param>
    /// <param name="sessionToken"></param>
    /// <param name="region"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void FillCredentials(
        string? profileName,
        out string accessKeyId,
        out string secretAccessKey,
        out string? sessionToken,
        out string? region
    )
    {
        if (_credIniFile is null)
        {
            throw new InvalidOperationException("Credential file does not exist");
        }

        profileName ??= DefaultProfileName;

        if (_credIniFile.SectionNames.All(s => s != profileName))
        {
            throw new InvalidOperationException(
                $"Profile '{profileName}' does not exist in credential file"
            );
        }

        var accessKeyId1 = _credIniFile[$"{profileName}.{AccessKeyIdKeyName}"];
        var secretAccessKey1 = _credIniFile[$"{profileName}.{SecretAccessKeyKeyName}"];

        if (string.IsNullOrWhiteSpace(accessKeyId1) || string.IsNullOrWhiteSpace(secretAccessKey1))
            throw new InvalidOperationException(
                $"{AccessKeyIdKeyName} and {SecretAccessKeyKeyName} keys are required"
            );

        accessKeyId = accessKeyId1;
        secretAccessKey = secretAccessKey1;
        sessionToken = _credIniFile[$"{profileName}.{SessionTokenKeyName}"];
        region = null;

        if (_configIniFile is null)
            return;

        // for default profile, the section is called "default", however for non-default profiles
        // the sections are called "profile <profileName>" (weird, huh?)
        var sectionName = profileName == DefaultProfileName ? "default" : $"profile {profileName}";
        region = _configIniFile[$"{sectionName}.region"];
    }
}
