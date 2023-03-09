using Azure.Identity;

namespace Microsoft.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{
	public static IConfigurationBuilder TryAddAzureKeyVault(this IConfigurationBuilder configuration, string? keyVaultName)
	{
		if (string.IsNullOrWhiteSpace(keyVaultName))
		{
			return configuration;
		}

		return configuration.AddAzureKeyVault(
			new Uri($"https://{keyVaultName}.vault.azure.net/"),
			new DefaultAzureCredential());
	}
}
