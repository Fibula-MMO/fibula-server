// -----------------------------------------------------------------
// <copyright file="KeyVaultSecretsProvider.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Providers.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading.Tasks;
    using Fibula.Providers.Contracts;
    using Fibula.Utilities.Validation;
    using global::Azure.Identity;
    using global::Azure.Security.KeyVault.Secrets;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Class that represents a secrets provider from Azure KeyVault.
    /// </summary>
    public class KeyVaultSecretsProvider : ISecretsProvider
    {
        /// <summary>
        /// The KeyVault client instance used by this provider.
        /// </summary>
        private readonly SecretClient keyVaultClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultSecretsProvider"/> class.
        /// </summary>
        /// <param name="secretsProviderOptions">A reference to the secrets provider options.</param>
        /// <param name="tokenProvider">A reference to the token provider service to use to obtain access to the vault.</param>
        /// <param name="logger">A logger for this provider.</param>
        public KeyVaultSecretsProvider(
            IOptions<KeyVaultSecretsProviderOptions> secretsProviderOptions,
            ITokenProvider tokenProvider,
            ILogger<KeyVaultSecretsProvider> logger)
        {
            secretsProviderOptions.ThrowIfNull(nameof(secretsProviderOptions));
            tokenProvider.ThrowIfNull(nameof(tokenProvider));

            DataAnnotationsValidator.ValidateObjectRecursive(secretsProviderOptions.Value);

            var options = new DefaultAzureCredentialOptions()
            {
                ExcludeInteractiveBrowserCredential = false,
                ExcludeSharedTokenCacheCredential = true,
                VisualStudioTenantId = "6780920b-09e8-4bbe-9736-77c1b346813c",
            };

            this.keyVaultClient = new SecretClient(new Uri(secretsProviderOptions.Value.VaultBaseUrl), new DefaultAzureCredential(options));
            this.Logger = logger;
        }

        /// <summary>
        /// Gets the logger in use by this provider.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets a Secret's value from the KeyVault as an asynchronous operation.
        /// </summary>
        /// <param name="secretName">The name of the secret to get the value of.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<SecureString> GetSecretValueAsync(string secretName)
        {
            secretName.ThrowIfNullOrWhiteSpace(nameof(secretName));

            this.Logger.LogDebug($"Retrieving secret '{secretName}' from Vault...");

            // Get the secret
            var secret = (await this.keyVaultClient.GetSecretAsync(secretName)).Value.Value;

            // Convert secret to SecureString. 'secret' object still keeps it in plain text.
            var secureSecret = new SecureString();

            foreach (char c in secret)
            {
                secureSecret.AppendChar(c);
            }

            secureSecret.MakeReadOnly();

            return secureSecret;
        }

        /// <summary>
        /// Retrieves a list of Secret identifiers from the secret store.
        /// </summary>
        /// <returns>A list of secret idetifiers.</returns>
        public async Task<IEnumerable<string>> ListSecretIdentifiers()
        {
            this.Logger.LogDebug($"Getting list of secret names from vault...");

            List<string> secrets = new List<string>();

            var secretProperties = this.keyVaultClient.GetPropertiesOfSecretsAsync();

            if (secretProperties != null)
            {
                await foreach (var secretProperty in secretProperties)
                {
                    secrets.Add(secretProperty.Name);
                }
            }

            return secrets;
        }
    }
}
