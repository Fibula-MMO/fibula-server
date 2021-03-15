// -----------------------------------------------------------------
// <copyright file="AccountConfiguration.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Plugins.Database.SqlServer.Configurations
{
    using Fibula.Definitions.Data.Entities;
    using Fibula.Plugins.Database.SqlServer.Constants;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Class that implements an entity type configuration for the <see cref="AccountEntity"/>.
    /// </summary>
    public class AccountConfiguration : IEntityTypeConfiguration<AccountEntity>
    {
        /// <summary>
        /// The name of the table referenced in this configuration.
        /// </summary>
        public const string TableName = "accounts";

        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<AccountEntity> builder)
        {
            // Set Keys
            builder.HasKey(a => a.Id).HasName("id");

            // Uniqueness
            builder.HasIndex(a => a.Number).IsUnique();

            // Relations and foreign keys.
            builder.HasMany(a => a.Characters).WithOne().HasForeignKey(c => c.AccountId);

            // Mapped columns.
            builder.Property(a => a.Number).HasColumnName("number").HasColumnType(ColumnTypeConstants.Int32);
            builder.Property(a => a.Password).HasColumnName("password");
            builder.Property(a => a.Email).HasColumnName("email");
            builder.Property(a => a.Creation).HasColumnName("creation").HasColumnType(ColumnTypeConstants.DateTimeOffset);
            builder.Property(a => a.CreationIp).HasColumnName("creationIp");
            builder.Property(a => a.SessionIp).HasColumnName("sessionIp");
            builder.Property(a => a.PremiumDays).HasColumnName("premiumDays").HasColumnType(ColumnTypeConstants.Int16);
            builder.Property(a => a.TrialOrBonusPremiumDays).HasColumnName("trialDays").HasColumnType(ColumnTypeConstants.Byte);
            builder.Property(a => a.BanishedUntil).HasColumnName("banishedUntil").HasColumnType(ColumnTypeConstants.DateTimeOffset);
            builder.Property(a => a.DeletedOn).HasColumnName("deletedOn").HasColumnType(ColumnTypeConstants.DateTimeOffset);

            // Unmapped columns.
            builder.Ignore(a => a.AccessLevel);
            builder.Ignore(a => a.Banished);
            builder.Ignore(a => a.Deleted);
            builder.Ignore(a => a.LastLogin);
            builder.Ignore(a => a.LastLoginIp);
            builder.Ignore(a => a.Premium);
            builder.Ignore(a => a.TrialPremium);
            builder.Ignore(a => a.SessionKey);

            // Set the table name.
            builder.ToTable(TableName);
        }
    }
}
