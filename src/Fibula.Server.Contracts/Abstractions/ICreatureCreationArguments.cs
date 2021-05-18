// -----------------------------------------------------------------
// <copyright file="ICreatureCreationArguments.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Abstractions
{
    using Fibula.Definitions.Data.Entities;
    using Fibula.Server.Contracts.Enumerations;

    /// <summary>
    /// Interface for arguments for creature creation.
    /// </summary>
    public interface ICreatureCreationArguments
    {
        /// <summary>
        /// Gets an id to use for the creature being created.
        /// </summary>
        uint PreselectedId { get; }

        /// <summary>
        /// Gets the type of creature being created.
        /// </summary>
        CreatureType Type { get; }

        /// <summary>
        /// Gets the metadata for the entity from which the creature is being created.
        /// </summary>
        CreatureEntity Metadata { get; }
    }
}
