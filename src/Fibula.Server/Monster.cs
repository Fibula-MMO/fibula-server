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

namespace Fibula.Server.Creatures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Enumerations;
    using Fibula.Definitions.Flags;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Constants;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Contracts.Structures;

    /// <summary>
    /// Class that represents all monsters in the game.
    /// </summary>
    public class Monster : CombatantCreature, IMonster
    {
        /// <summary>
        /// An object used as the lock to semaphore access to <see cref="hostileCombatants"/>.
        /// </summary>
        private readonly object hostileCombatantsLock;

        /// <summary>
        /// Stores the set of combatants that are deemed hostile to this monster.
        /// </summary>
        private readonly ISet<ICombatant> hostileCombatants;

        /// <summary>
        /// Initializes a new instance of the <see cref="Monster"/> class.
        /// </summary>
        /// <param name="monsterType">The type of this monster.</param>
        public Monster(MonsterTypeEntity monsterType)
            : base(monsterType)
        {
            this.Type = monsterType;
            this.Outfit = monsterType.OriginalOutfit;

            this.Stats[CreatureStat.BaseSpeed].Set(monsterType.BaseSpeed == 0 ? 0 : (uint)(2 * monsterType.BaseSpeed) + 80);

            this.BloodType = monsterType.BloodType;
            this.ChaseMode = this.AutoAttackRange > 1 ? ChaseMode.KeepDistance : ChaseMode.Chase;
            this.FightMode = FightMode.FullAttack;

            this.hostileCombatants = new HashSet<ICombatant>();
            this.hostileCombatantsLock = new object();

            this.InitializeSkills();
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
            get => (ushort)(this.Stats[CreatureStat.BaseSpeed].Current == 0 ? 0 : (ushort)(this.Stats[CreatureStat.BaseSpeed].Current + (2 * this.VariableSpeed)));
        }

        /// <summary>
        /// Gets the range that the auto attack has.
        /// </summary>
        public override byte AutoAttackRange => (byte)(this.Type.HasCreatureFlag(CreatureFlag.KeepsDistance) ? MonsterConstants.DefaultDistanceFightingAttackRange : MonsterConstants.DefaultMeleeFightingAttackRange);

        /// <summary>
        /// Starts tracking another <see cref="ICombatant"/>.
        /// </summary>
        /// <param name="otherCombatant">The other combatant, now in view.</param>
        public override void AddToCombatList(ICombatant otherCombatant)
        {
            if (this == otherCombatant || !(otherCombatant is IPlayer))
            {
                return;
            }

            lock (this.hostileCombatantsLock)
            {
                this.hostileCombatants.Add(otherCombatant);

                if (this.hostileCombatants.Count == 1)
                {
                    this.ChaseMode = this.Type.HasCreatureFlag(CreatureFlag.KeepsDistance) ? ChaseMode.KeepDistance : ChaseMode.Chase;
                    this.SetAttackTarget(otherCombatant);
                }
            }
        }

        /// <summary>
        /// Stops tracking another <see cref="ICombatant"/>.
        /// </summary>
        /// <param name="otherCombatant">The other combatant, now in view.</param>
        public override void RemoveFromCombatList(ICombatant otherCombatant)
        {
            if (this == otherCombatant)
            {
                return;
            }

            lock (this.hostileCombatantsLock)
            {
                this.hostileCombatants.Remove(otherCombatant);

                if (otherCombatant == this.AutoAttackTarget)
                {
                    this.SetAttackTarget(this.hostileCombatants.Count > 0 ? this.hostileCombatants.First() : null);
                }
            }
        }

        /// <summary>
        /// Applies damage modifiers to the damage information provided.
        /// </summary>
        /// <param name="damageInfo">The damage information.</param>
        protected override void ApplyDamageModifiers(ref DamageInfo damageInfo)
        {
            var rng = new Random();

            // 75% chance to block it?
            if (this.Stats[CreatureStat.DefensePoints].Current > 0 && rng.Next(4) > 0)
            {
                damageInfo.Effect = AnimatedEffect.Puff;
                damageInfo.Damage = 0;
            }

            // 25% chance to hit the armor...
            if (rng.Next(4) == 0)
            {
                damageInfo.Effect = AnimatedEffect.SparkYellow;
                damageInfo.Damage = 0;
            }
        }

        private void InitializeSkills()
        {
            // make a copy of the type we are based on...
            foreach (var kvp in this.Type.Skills)
            {
                (int defaultLevel, int currentLevel, int maximumLevel, uint targetForNextLevel, uint targetIncreaseFactor, byte increasePerLevel) = kvp.Value;

                this.Skills[kvp.Key] = new MonsterSkill(kvp.Key, defaultLevel, currentLevel, maximumLevel, 0, targetForNextLevel, targetIncreaseFactor, increasePerLevel);
                this.Skills[kvp.Key].CountChanged += this.RaiseSkillChange;
                this.Skills[kvp.Key].LevelChanged += this.RaiseSkillChange;
                this.Skills[kvp.Key].PercentChanged += this.RaiseSkillChange;
            }

            // Add experience yield as a skill
            if (!this.Skills.ContainsKey(SkillType.Experience))
            {
                this.Skills[SkillType.Experience] = new MonsterSkill(SkillType.Experience, 1, 0, int.MaxValue, Math.Min(uint.MaxValue, this.Type.BaseExperienceYield), Math.Min(uint.MaxValue, this.Type.BaseExperienceYield) * 2, 1100, 1);
                this.Skills[SkillType.Experience].CountChanged += this.RaiseSkillChange;
                this.Skills[SkillType.Experience].LevelChanged += this.RaiseSkillChange;
                this.Skills[SkillType.Experience].PercentChanged += this.RaiseSkillChange;
            }

            if (!this.Skills.ContainsKey(SkillType.Shield))
            {
                this.Skills[SkillType.Shield] = new MonsterSkill(SkillType.Shield, Math.Min(int.MaxValue, this.Type.BaseDefense), 0, int.MaxValue, 0, 100, 1100, 1);
                this.Skills[SkillType.Shield].CountChanged += this.RaiseSkillChange;
                this.Skills[SkillType.Shield].LevelChanged += this.RaiseSkillChange;
                this.Skills[SkillType.Shield].PercentChanged += this.RaiseSkillChange;
            }

            if (!this.Skills.ContainsKey(SkillType.NoWeapon))
            {
                this.Skills[SkillType.NoWeapon] = new MonsterSkill(SkillType.NoWeapon, Math.Min(int.MaxValue, this.Type.BaseAttack), 0, int.MaxValue, 0, 100, 1100, 1);
                this.Skills[SkillType.NoWeapon].CountChanged += this.RaiseSkillChange;
                this.Skills[SkillType.NoWeapon].LevelChanged += this.RaiseSkillChange;
                this.Skills[SkillType.NoWeapon].PercentChanged += this.RaiseSkillChange;
            }
        }
    }
}
