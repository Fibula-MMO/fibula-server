﻿// -----------------------------------------------------------------
// <copyright file="FibulaCosmosDbContext.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Plugins.Database.CosmosDB
{
    using System.Security;
    using Fibula.Data.Contracts.Abstractions;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Providers.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Class that represents a CosmosDB context.
    /// </summary>
    public class FibulaCosmosDbContext : DbContext, IFibulaDbContext
    {
        /// <summary>
        /// The name for the container which holds account information.
        /// </summary>
        private const string AccountsContainerName = "Accounts";

        /// <summary>
        /// The name for the container which holds character information.
        /// </summary>
        private const string CharactersContainerName = "Characters";

        /// <summary>
        /// A lock object for <see cref="accountEndpoint"/> initialization.
        /// </summary>
        private static readonly object AccountEndpointLock = new object();

        /// <summary>
        /// A lock object for <see cref="accountkey"/> initialization.
        /// </summary>
        private static readonly object AccountkeyLock = new object();

        /// <summary>
        /// Holds the account endpoint as a secure string in memory.
        /// </summary>
        private static SecureString accountEndpoint;

        /// <summary>
        /// Holds the account key as a secure string in memory.
        /// </summary>
        private static SecureString accountkey;

        /// <summary>
        /// Initializes a new instance of the <see cref="FibulaCosmosDbContext"/> class.
        /// </summary>
        /// <param name="cosmosDbContextOptions">A reference to the CosmosDb context options.</param>
        /// <param name="secretsProvider">A reference to the secrets provider.</param>
        public FibulaCosmosDbContext(
            IOptions<FibulaCosmosDbContextOptions> cosmosDbContextOptions,
            ISecretsProvider secretsProvider)
        {
            cosmosDbContextOptions.ThrowIfNull(nameof(cosmosDbContextOptions));
            secretsProvider.ThrowIfNull(nameof(secretsProvider));

            DataAnnotationsValidator.ValidateObjectRecursive(cosmosDbContextOptions.Value);

            this.Options = cosmosDbContextOptions.Value;
            this.SecretsProvider = secretsProvider;
        }

        /// <summary>
        /// Gets the configuration options.
        /// </summary>
        public FibulaCosmosDbContextOptions Options { get; }

        /// <summary>
        /// Gets the reference to the secrets provider.
        /// </summary>
        public ISecretsProvider SecretsProvider { get; }

        private string AccountEndpoint
        {
            get
            {
                if (accountEndpoint == null)
                {
                    lock (AccountEndpointLock)
                    {
                        if (accountEndpoint == null)
                        {
                            // Attempt to retrieve secret using the provider.
                            accountEndpoint = this.SecretsProvider.GetSecretValueAsync(this.Options.AccountEndpointSecretName).Result;
                        }
                    }
                }

                // One-liner hack to decode a secure string.
                return new System.Net.NetworkCredential(string.Empty, accountEndpoint).Password;
            }
        }

        private string AccountKey
        {
            get
            {
                if (accountkey == null)
                {
                    lock (AccountkeyLock)
                    {
                        if (accountkey == null)
                        {
                            // Attempt to retrieve secret using the provider.
                            accountkey = this.SecretsProvider.GetSecretValueAsync(this.Options.AccountKeySecretName).Result;
                        }
                    }
                }

                // One-liner hack to decode a secure string.
                return new System.Net.NetworkCredential(string.Empty, accountkey).Password;
            }
        }

        /// <summary>
        /// Gets this context as the <see cref="DbContext"/>.
        /// </summary>
        /// <returns>This instance casted as <see cref="DbContext"/>.</returns>
        public DbContext AsDbContext()
        {
            return this;
        }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // This actually sets up the link to CosmosDb
            optionsBuilder.UseCosmos(this.AccountEndpoint, this.AccountKey, this.Options.DatabaseName);
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountEntity>()
                .ToContainer(AccountsContainerName)
                .HasPartitionKey(a => a.Id);

            modelBuilder.Entity<CharacterEntity>()
                .ToContainer(CharactersContainerName)
                .HasPartitionKey(c => c.Id);

            // TODO: Add more entities here.
        }
    }
}
