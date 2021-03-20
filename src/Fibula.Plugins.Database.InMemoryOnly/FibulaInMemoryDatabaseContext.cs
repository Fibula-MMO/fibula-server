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
    using System.Collections.Generic;
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
            var characterId = Guid.NewGuid().ToString();

            // Accounts
            modelBuilder.Entity<AccountEntity>().HasKey(a => a.Id);
            modelBuilder.Entity<AccountEntity>().HasIndex(a => a.Number).IsUnique();
            modelBuilder.Entity<AccountEntity>().HasMany(a => a.Characters).WithOne().HasForeignKey(c => c.AccountId);

            modelBuilder.Entity<AccountEntity>().Ignore(a => a.AccessLevel);
            modelBuilder.Entity<AccountEntity>().Ignore(a => a.Banished);
            modelBuilder.Entity<AccountEntity>().Ignore(a => a.Deleted);
            modelBuilder.Entity<AccountEntity>().Ignore(a => a.LastLogin);
            modelBuilder.Entity<AccountEntity>().Ignore(a => a.LastLoginIp);
            modelBuilder.Entity<AccountEntity>().Ignore(a => a.Premium);
            modelBuilder.Entity<AccountEntity>().Ignore(a => a.TrialPremium);
            modelBuilder.Entity<AccountEntity>().Ignore(a => a.SessionKey);

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
                    Characters = new List<CharacterEntity>(),
                });

            // Characters
            modelBuilder.Entity<CharacterEntity>().HasKey(c => c.Id);
            modelBuilder.Entity<CharacterEntity>().HasIndex(c => c.Name).IsUnique();
            modelBuilder.Entity<CharacterEntity>().HasMany(a => a.Stats).WithOne().HasForeignKey(c => c.CharacterId);

            modelBuilder.Entity<CharacterEntity>().Ignore(c => c.Article);
            modelBuilder.Entity<CharacterEntity>().Ignore(c => c.Corpse);
            modelBuilder.Entity<CharacterEntity>().Ignore(c => c.CurrentHitpoints);
            modelBuilder.Entity<CharacterEntity>().Ignore(c => c.CurrentManapoints);
            modelBuilder.Entity<CharacterEntity>().Ignore(c => c.MaxHitpoints);
            modelBuilder.Entity<CharacterEntity>().Ignore(c => c.MaxManapoints);

            modelBuilder.Entity<CharacterEntity>()
                .HasData(new CharacterEntity()
                {
                    Id = characterId,
                    Name = "Test",
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
                    LastOutfit = new Outfit
                    {
                        Id = 128,
                        Head = 114,
                        Body = 114,
                        Legs = 114,
                        Feet = 114,
                    },
                    Stats = new List<CharacterStatEntity>(),
                    StartingLocation = new Location() { X = 32369, Y = 32241, Z = 7 },
                    LastLocation = new Location() { X = 32369, Y = 32241, Z = 7 },
                });

            // Character Stats
            modelBuilder.Entity<CharacterStatEntity>().HasKey(s => new { s.CharacterId, s.Type });

            modelBuilder.Entity<CharacterStatEntity>()
                .HasData(new CharacterStatEntity()
                {
                    CharacterId = characterId,
                    Type = CharacterStat.HitPoints,
                    Current = 1000,
                    Maximum = 1000,
                    Default = 100,
                    Minimum = 0,
                });

            modelBuilder.Entity<CharacterStatEntity>()
                .HasData(new CharacterStatEntity()
                {
                    CharacterId = characterId,
                    Type = CharacterStat.ManaPoints,
                    Current = 100,
                    Maximum = 100,
                    Default = 100,
                    Minimum = 0,
                });

            modelBuilder.Entity<CharacterStatEntity>()
                .HasData(new CharacterStatEntity()
                {
                    CharacterId = characterId,
                    Type = CharacterStat.BaseSpeed,
                    Current = 220,
                    Maximum = 220,
                    Default = 50,
                    Minimum = 0,
                });

            modelBuilder.Entity<CharacterStatEntity>()
                .HasData(new CharacterStatEntity()
                {
                    CharacterId = characterId,
                    Type = CharacterStat.CarryStrength,
                    Current = 100,
                    Maximum = 100,
                    Default = 100,
                    Minimum = 0,
                });
        }
    }
}
