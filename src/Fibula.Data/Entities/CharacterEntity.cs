// -----------------------------------------------------------------
// <copyright file="CharacterEntity.cs" company="2Dudes">
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
    using System;
    using Fibula.Data.Entities.Contracts.Abstractions;
    using Fibula.Data.Entities.Contracts.Structs;
    using Fibula.Definitions.Enumerations;

    /// <summary>
    /// Class that represents a character entity.
    /// </summary>
    public class CharacterEntity : BaseEntity, ICharacterEntity
    {
        /// <summary>
        /// Gets or sets the character's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the article to use with the character's name.
        /// </summary>
        public string Article => string.Empty;

        /// <summary>
        /// Gets or sets the id of the account which this character belongs to.
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the world where the character exists in.
        /// </summary>
        public string World { get; set; }

        /// <summary>
        /// Gets or sets the character's chosen gender.
        /// </summary>
        public byte Gender { get; set; }

        /// <summary>
        /// Gets or sets the character's profession.
        /// </summary>
        public ProfessionType Profession { get; set; }

        /// <summary>
        /// Gets or sets this character's creation date and time.
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Gets or sets the last observed date and time that this character successfully loged in.
        /// </summary>
        public DateTimeOffset? LastLogin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this character is currently online.
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Gets the current hitpoints of the character.
        /// </summary>
        public ushort CurrentHitpoints => this.MaxHitpoints;

        /// <summary>
        /// Gets the max hitpoints of the character.
        /// </summary>
        public ushort MaxHitpoints => 500;

        /// <summary>
        /// Gets the current manapoints of the character.
        /// </summary>
        public ushort CurrentManapoints => this.MaxManapoints;

        /// <summary>
        /// Gets the max manapoints to create the creature with.
        /// </summary>
        public ushort MaxManapoints => 0;

        /// <summary>
        /// Gets the corpse id to give to the creature.
        /// </summary>
        public ushort Corpse => 4240;

        /// <summary>
        /// Gets or sets the outfit look for the creature.
        /// </summary>
        public Outfit Outfit { get; set; }
    }
}
