// -----------------------------------------------------------------
// <copyright file="CreatureCreationArguments.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Models
{
    using Fibula.Definitions.Data.Entities;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Enumerations;

    /// <summary>
    /// Class that represents creation arguments for creatures.
    /// </summary>
    public class CreatureCreationArguments : ICreatureCreationArguments
    {
        /// <summary>
        /// Gets or sets an id to use for the creature being created.
        /// </summary>
        public uint PreselectedId { get; set; }

        /// <summary>
        /// Gets or sets the type of creature being created.
        /// </summary>
        public CreatureType Type { get; set; }

        /// <summary>
        /// Gets or sets the metadata for the creature being created.
        /// </summary>
        public CreatureEntity Metadata { get; set; }
    }
}
