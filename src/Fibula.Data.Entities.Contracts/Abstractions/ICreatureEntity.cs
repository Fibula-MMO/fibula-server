// -----------------------------------------------------------------
// <copyright file="ICreatureEntity.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Data.Entities.Contracts.Abstractions
{
    using Fibula.Data.Entities.Contracts.Structs;

    /// <summary>
    /// Interface for creature creation metadata.
    /// </summary>
    public interface ICreatureEntity : IIdentifiableEntity
    {
        /// <summary>
        /// Gets the article to prefix the creature name's with.
        /// </summary>
        string Article { get; }

        /// <summary>
        /// Gets the name to use for the creature.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the current hitpoints to create the creature with.
        /// </summary>
        ushort CurrentHitpoints { get; }

        /// <summary>
        /// Gets the max hitpoints to create the creature with.
        /// </summary>
        ushort MaxHitpoints { get; }

        /// <summary>
        /// Gets the current manapoints to create the creature with.
        /// </summary>
        ushort CurrentManapoints { get; }

        /// <summary>
        /// Gets the max manapoints to create the creature with.
        /// </summary>
        ushort MaxManapoints { get; }

        /// <summary>
        /// Gets the corpse id to give to the creature.
        /// </summary>
        ushort Corpse { get; }

        /// <summary>
        /// Gets the outfit look for the creature.
        /// </summary>
        Outfit Outfit { get; }
    }
}
