// -----------------------------------------------------------------
// <copyright file="CharacterConfiguration.cs" company="2Dudes">
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
    using Fibula.Plugins.Database.SqlServer.Converters;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Class that implements an entity type configuration for the <see cref="CharacterEntity"/>.
    /// </summary>
    public class CharacterConfiguration : IEntityTypeConfiguration<CharacterEntity>
    {
        /// <summary>
        /// The name of the table referenced in this configuration.
        /// </summary>
        public const string TableName = "characters";

        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<CharacterEntity> builder)
        {
            // Set Keys
            builder.HasKey(a => a.Id).HasName("id");

            // Uniqueness
            builder.HasIndex(c => c.Name).IsUnique();

            // Relations and foreign keys.
            builder.HasMany(a => a.Stats).WithOne().HasForeignKey(c => c.CharacterId);

            // Map properties
            builder.Property(c => c.World).HasColumnName("world");
            builder.Property(c => c.Type).HasColumnName("type").HasColumnType(ColumnTypeConstants.Byte);
            builder.Property(c => c.Profession).HasColumnName("profession").HasColumnType(ColumnTypeConstants.Byte);
            builder.Property(c => c.Creation).HasColumnName("creation").HasColumnType(ColumnTypeConstants.DateTimeOffset);
            builder.Property(c => c.LastLogin).HasColumnName("lastLogin").HasColumnType(ColumnTypeConstants.DateTimeOffset);
            builder.Property(c => c.IsOnline).HasColumnName("isOnline").HasConversion(new BoolConverter());
            builder.Property(c => c.DeletedOn).HasColumnName("deletedOn").HasColumnType(ColumnTypeConstants.DateTimeOffset);
            builder.Property(c => c.BanishedUntil).HasColumnName("banishedUntil").HasColumnType(ColumnTypeConstants.DateTimeOffset);
            builder.Property(c => c.StartingLocation).HasColumnName("startingLocation").HasConversion(new LocationConverter());
            builder.Property(c => c.LastLocation).HasColumnName("lastLocation").HasConversion(new LocationConverter());

            // Unmapped columns.
            builder.Ignore(c => c.OriginalOutfit);
            builder.Ignore(c => c.LastOutfit);
            builder.Ignore(c => c.Article);
            builder.Ignore(c => c.Corpse);
            builder.Ignore(c => c.CurrentHitpoints);
            builder.Ignore(c => c.CurrentManapoints);
            builder.Ignore(c => c.MaxHitpoints);
            builder.Ignore(c => c.MaxManapoints);

            builder.ToTable(TableName);
        }
    }
}
