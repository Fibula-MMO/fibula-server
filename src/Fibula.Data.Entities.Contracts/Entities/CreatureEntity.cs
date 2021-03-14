// -----------------------------------------------------------------
// <copyright file="CreatureEntity.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Data.Entities
{
    using Fibula.Data.Entities.Contracts.Structs;

    /// <summary>
    /// Class that represents a creature entity.
    /// </summary>
    public abstract class CreatureEntity : BaseEntity
    {
        /// <summary>
        /// Gets or sets the article to prefix the creature name's with.
        /// </summary>
        public abstract string Article { get; set; }

        /// <summary>
        /// Gets or sets the name to use for the creature.
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Gets or sets the max hitpoints to create the creature with.
        /// </summary>
        public abstract ushort MaxHitpoints { get; set; }

        /// <summary>
        /// Gets or sets the max manapoints to create the creature with.
        /// </summary>
        public abstract ushort MaxManapoints { get; set; }

        /// <summary>
        /// Gets or sets the corpse id to give to the creature.
        /// </summary>
        public abstract ushort Corpse { get; set; }

        /// <summary>
        /// Gets the current hitpoints to create the creature with.
        /// </summary>
        public virtual ushort CurrentHitpoints { get; }

        /// <summary>
        /// Gets the current manapoints to create the creature with.
        /// </summary>
        public virtual ushort CurrentManapoints { get; }

        /// <summary>
        /// Gets or sets the outfit look for the creature.
        /// </summary>
        public virtual Outfit Outfit { get; set; }
    }
}
