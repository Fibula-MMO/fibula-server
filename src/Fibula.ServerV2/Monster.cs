// -----------------------------------------------------------------
// <copyright file="Monster.cs" company="2Dudes">
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
    using Fibula.Definitions.Data.Entities;
    using Fibula.ServerV2.Contracts.Abstractions;

    /// <summary>
    /// Class that represents all monsters in the game.
    /// </summary>
    public class Monster : Creature, IMonster
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Monster"/> class.
        /// </summary>
        /// <param name="monsterType">The type of this monster.</param>
        public Monster(MonsterTypeEntity monsterType)
            : base(monsterType)
        {
            this.Type = monsterType;
            this.Outfit = monsterType.OriginalOutfit;

            this.BloodType = monsterType.BloodType;
        }

        /// <summary>
        /// Gets the type of this monster.
        /// </summary>
        public MonsterTypeEntity Type { get; }

        /// <summary>
        /// Gets the experience yielded when this monster dies.
        /// </summary>
        public uint ExperienceToYield => 1; // Convert.ToUInt32(Math.Min(uint.MaxValue, this.Skills[SkillType.Experience].CurrentCount));

        /// <summary>
        /// Gets the monster speed.
        /// </summary>
        public override ushort Speed
        {
            // get => (ushort)(this.Stats[CreatureStat.BaseSpeed].Current == 0 ? 0 : (ushort)(this.Stats[CreatureStat.BaseSpeed].Current + (2 * this.VariableSpeed)));
            get => (ushort)(50 + (2 * this.VariableSpeed));
        }
    }
}
