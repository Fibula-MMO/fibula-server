// -----------------------------------------------------------------
// <copyright file="FibulaInMemoryDatabaseContext.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Plugins.Database.InMemoryOnly
{
    using System;
    using Fibula.Data.Contracts.Abstractions;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Class that represents a context for an in-memory database.
    /// </summary>
    public class FibulaInMemoryDatabaseContext : DbContext, IFibulaDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FibulaInMemoryDatabaseContext"/> class.
        /// </summary>
        /// <param name="options">The options to initialize this context with.</param>
        public FibulaInMemoryDatabaseContext(DbContextOptions<FibulaInMemoryDatabaseContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the set of accounts.
        /// </summary>
        public DbSet<AccountEntity> Accounts { get; set; }

        /// <summary>
        /// Gets or sets the set of characters.
        /// </summary>
        public DbSet<CharacterEntity> Characters { get; set; }

        /// <summary>
        /// Gets this context as the <see cref="DbContext"/>.
        /// </summary>
        /// <returns>This instance casted as <see cref="DbContext"/>.</returns>
        public DbContext AsDbContext()
        {
            return this;
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var accountId = Guid.NewGuid().ToString();

            modelBuilder.Entity<AccountEntity>()
                .HasData(new AccountEntity()
                {
                    Id = accountId,
                    AccessLevel = 0,
                    Banished = false,
                    BanishedUntil = default,
                    Creation = DateTimeOffset.UtcNow,
                    CreationIp = "127.0.0.1",
                    Deleted = false,
                    Email = "someone@email.com",
                    LastLogin = DateTimeOffset.UtcNow,
                    LastLoginIp = null,
                    Number = 1,
                    Password = "1",
                    Premium = false,
                    PremiumDays = 0,
                });

            modelBuilder.Entity<CharacterEntity>()
                .HasData(new CharacterEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Alpha",
                    AccountId = accountId,
                    Profession = ProfessionType.None,
                    World = "Fibula",
                    Type = CharacterType.HumanMale,
                    Creation = DateTimeOffset.UtcNow,
                    LastLogin = DateTimeOffset.UtcNow,
                    IsOnline = false,
                    OriginalOutfit = new Outfit
                    {
                        Id = 128,
                        Head = 114,
                        Body = 114,
                        Legs = 114,
                        Feet = 114,
                    },
                });

            modelBuilder.Entity<CharacterEntity>()
                .HasData(new CharacterEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Beta",
                    AccountId = accountId,
                    Profession = ProfessionType.None,
                    World = "Fibula",
                    Type = CharacterType.HumanMale,
                    Creation = DateTimeOffset.UtcNow,
                    LastLogin = DateTimeOffset.UtcNow,
                    IsOnline = false,
                    OriginalOutfit = new Outfit
                    {
                        Id = 128,
                        Head = 114,
                        Body = 114,
                        Legs = 114,
                        Feet = 114,
                    },
                });

            modelBuilder.Entity<CharacterEntity>()
                .HasData(new CharacterEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Charlie",
                    AccountId = accountId,
                    Profession = ProfessionType.None,
                    World = "Fibula",
                    Type = CharacterType.HumanMale,
                    Creation = DateTimeOffset.UtcNow,
                    LastLogin = DateTimeOffset.UtcNow,
                    IsOnline = false,
                    OriginalOutfit = new Outfit
                    {
                        Id = 128,
                        Head = 114,
                        Body = 114,
                        Legs = 114,
                        Feet = 114,
                    },
                });
        }
    }
}
