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
    using Fibula.Data.Entities.Contracts.Structs;
    using Fibula.Definitions.Enumerations;

    /// <summary>
    /// Class that represents a character entity.
    /// </summary>
    public class CharacterEntity : CreatureEntity
    {
        /// <summary>
        /// Gets or sets the character's name.
        /// </summary>
        public override string Name { get; set; }

        /// <summary>
        /// Gets or sets the article to use with the character's name.
        /// </summary>
        public override string Article
        {
            get { return string.Empty; }
            set { return; }
        }

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
        public override ushort CurrentHitpoints => this.MaxHitpoints;

        /// <summary>
        /// Gets or sets the max hitpoints of the character.
        /// </summary>
        public override ushort MaxHitpoints
        {
            get { return 500; }
            set { return; }
        }

        /// <summary>
        /// Gets the current manapoints of the character.
        /// </summary>
        public override ushort CurrentManapoints => this.MaxManapoints;

        /// <summary>
        /// Gets or sets the max manapoints to create the creature with.
        /// </summary>
        public override ushort MaxManapoints
        {
            get { return 0; }
            set { return; }
        }

        /// <summary>
        /// Gets or sets the corpse id to give to the creature.
        /// </summary>
        public override ushort Corpse
        {
            get { return 4240; }
            set { return; }
        }

        /// <summary>
        /// Gets or sets the outfit look for the creature.
        /// </summary>
        public override Outfit Outfit { get; set; }
    }
}
