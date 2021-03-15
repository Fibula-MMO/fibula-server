// -----------------------------------------------------------------
// <copyright file="FibulaSqlServerContext.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Plugins.Database.SqlServer
{
    using System.Security;
    using Fibula.Data.Contracts.Abstractions;
    using Fibula.Plugins.Database.SqlServer.Configurations;
    using Fibula.Providers.Contracts;
    using Fibula.Utilities.Validation;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Class that represents a CosmosDB context.
    /// </summary>
    public class FibulaSqlServerContext : DbContext, IFibulaDbContext
    {
        /// <summary>
        /// A lock object for <see cref="connectionString"/> initialization.
        /// </summary>
        private static readonly object ConnectionStringLock = new object();

        /// <summary>
        /// Holds the account endpoint as a secure string in memory.
        /// </summary>
        private static SecureString connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="FibulaSqlServerContext"/> class.
        /// </summary>
        /// <param name="cosmosDbContextOptions">A reference to the CosmosDb context options.</param>
        /// <param name="secretsProvider">A reference to the secrets provider.</param>
        public FibulaSqlServerContext(
            IOptions<FibulaSqlServerContextOptions> cosmosDbContextOptions,
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
        public FibulaSqlServerContextOptions Options { get; }

        /// <summary>
        /// Gets the reference to the secrets provider.
        /// </summary>
        public ISecretsProvider SecretsProvider { get; }

        private string ConnectionString
        {
            get
            {
                if (connectionString == null)
                {
                    lock (ConnectionStringLock)
                    {
                        if (connectionString == null)
                        {
                            // Attempt to retrieve secret using the provider.
                            connectionString = this.SecretsProvider.GetSecretValueAsync(this.Options.ConnectionStringSecretName).Result;
                        }
                    }
                }

                // One-liner hack to decode a secure string.
                return new System.Net.NetworkCredential(string.Empty, connectionString).Password;
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
            optionsBuilder.UseSqlServer(this.ConnectionString, (options) =>
            {
                options.EnableRetryOnFailure();
            });
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("fibula_v1");

            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterStatConfiguration());
        }
    }
}
