// -----------------------------------------------------------------
// <copyright file="Player.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Creatures
{
    using Fibula.Definitions.Constants;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Enumerations;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents all players in the game.
    /// </summary>
    public class Player : Creature, IPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="characterEntity">The player's corresponding character entity lodaded from storage.</param>
        /// <param name="preselectedId">Optinal. A pre-selected id for the player.</param>
        public Player(CharacterEntity characterEntity, uint preselectedId = 0)
            : base(characterEntity, preselectedId)
        {
            characterEntity.ThrowIfNull(nameof(characterEntity));

            this.CharacterId = characterEntity.Id;

            this.Outfit = characterEntity.OriginalOutfit;

            // Hard-coded stuff.
            this.EmittedLightLevel = LightConstants.TorchLevel;
            this.EmittedLightColor = LightConstants.OrangeColor;
        }

        /// <summary>
        /// Gets the player's character id.
        /// </summary>
        public string CharacterId { get; }

        /// <summary>
        /// Gets the player's permissions level.
        /// </summary>
        public byte PermissionsLevel { get; }

        /// <summary>
        /// Gets the player's profession.
        /// </summary>
        public ProfessionType Profession { get; }

        /// <summary>
        /// Gets this player's speed.
        /// </summary>
        public override ushort Speed
        {
            // get => (ushort)(this.Stats[CreatureStat.BaseSpeed].Current + (2 * this.VariableSpeed));
            get => (ushort)(220 + (2 * this.VariableSpeed));
        }

        /// <summary>
        /// Selects and returns a new in-game id for a player.
        /// </summary>
        /// <returns>The picked id.</returns>
        public static uint ReserveNewId()
        {
            return Creature.NewCreatureId();
        }
    }
}
