// -----------------------------------------------------------------
// <copyright file="CharacterStatConfiguration.cs" company="2Dudes">
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
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Class that implements an entity type configuration for the <see cref="CharacterStatEntity"/>.
    /// </summary>
    public class CharacterStatConfiguration : IEntityTypeConfiguration<CharacterStatEntity>
    {
        /// <summary>
        /// The name of the table referenced in this configuration.
        /// </summary>
        public const string TableName = "character_stats";

        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<CharacterStatEntity> builder)
        {
            // Set Keys
            builder.HasKey(c => new { c.CharacterId, c.Type });

            // Map properties
            builder.Property(c => c.Maximum).HasColumnName("maximum");
            builder.Property(c => c.Minimum).HasColumnName("minimum");
            builder.Property(c => c.Default).HasColumnName("default");
            builder.Property(c => c.Current).HasColumnName("current");

            builder.ToTable(TableName);
        }
    }
}
