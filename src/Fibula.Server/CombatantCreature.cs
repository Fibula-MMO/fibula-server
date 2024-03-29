﻿// -----------------------------------------------------------------
// <copyright file="CombatantCreature.cs" company="2Dudes">
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Constants;
    using Fibula.Server.Contracts.Delegates;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Contracts.Extensions;
    using Fibula.Server.Contracts.Structures;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents all creatures in the game.
    /// </summary>
    public abstract class CombatantCreature : Creature, ICombatant
    {
        /// <summary>
        /// Stores the map of combatants to the damage taken from them for the current combat session.
        /// </summary>
        private readonly ConcurrentDictionary<uint, uint> combatSessionDamageTakenMap;

        /// <summary>
        /// The base of how fast a combatant can earn an attack credit per combat round.
        /// </summary>
        private readonly decimal baseAttackSpeed;

        /// <summary>
        /// The base of how fast a combatant can earn a defense credit per combat round.
        /// </summary>
        private readonly decimal baseDefenseSpeed;

        /// <summary>
        /// Stores the set of combatants currently attacking this combatant for the current combat session.
        /// </summary>
        private readonly ISet<ICombatant> combatSessionAttackedBy;

        /// <summary>
        /// Stores the creatures that this creature is aware of.
        /// </summary>
        private readonly Dictionary<ICreature, AwarenessLevel> creatureAwarenessMap;

        /// <summary>
        /// The buff for attack speed.
        /// </summary>
        private decimal attackSpeedBuff;

        /// <summary>
        /// The buff for defense speed.
        /// </summary>
        private decimal defenseSpeedBuff;

        /// <summary>
        /// Initializes a new instance of the <see cref="CombatantCreature"/> class.
        /// </summary>
        /// <param name="creationMetadata">The metadata for this player.</param>
        /// <param name="preselectedId">Optional. A pre-selected id to use for the combatant. Picks a new id if none (or 0) is specified.</param>
        /// <param name="baseAttackSpeed">
        /// Optional. The base attack speed for this creature.
        /// Bounded between [<see cref="CombatConstants.MinimumCombatSpeedFactor"/>, <see cref="CombatConstants.MaximumCombatSpeedFactor"/>] inclusive.
        /// Defaults to <see cref="CombatConstants.DefaultAttackSpeedFactor"/>.
        /// </param>
        /// <param name="baseDefenseSpeed">
        /// Optional. The base defense speed for this creature.
        /// Bounded between [<see cref="CombatConstants.MinimumCombatSpeedFactor"/>, <see cref="CombatConstants.MaximumCombatSpeedFactor"/>] inclusive.
        /// Defaults to <see cref="CombatConstants.DefaultDefenseSpeedFactor"/>.
        /// </param>
        protected CombatantCreature(
            CreatureEntity creationMetadata,
            uint preselectedId = 0,
            decimal baseAttackSpeed = CombatConstants.DefaultAttackSpeedFactor,
            decimal baseDefenseSpeed = CombatConstants.DefaultDefenseSpeedFactor)
            : base(creationMetadata, preselectedId)
        {
            // Normalize combat speeds.
            this.baseAttackSpeed = Math.Min(CombatConstants.MaximumCombatSpeedFactor, Math.Max(CombatConstants.MinimumCombatSpeedFactor, baseAttackSpeed));
            this.baseDefenseSpeed = Math.Min(CombatConstants.MaximumCombatSpeedFactor, Math.Max(CombatConstants.MinimumCombatSpeedFactor, baseDefenseSpeed));

            this.combatSessionDamageTakenMap = new ConcurrentDictionary<uint, uint>();
            this.combatSessionAttackedBy = new HashSet<ICombatant>();

            this.creatureAwarenessMap = new Dictionary<ICreature, AwarenessLevel>();

            this.Skills = new Dictionary<SkillType, ISkill>();

            this.Stats.Add(CreatureStat.AttackPoints, new Stat(CreatureStat.AttackPoints, 1, CombatConstants.DefaultMaximumAttackCredits));
            this.Stats[CreatureStat.AttackPoints].ValueChanged += this.RaiseStatChange;

            this.Stats.Add(CreatureStat.DefensePoints, new Stat(CreatureStat.DefensePoints, 2, CombatConstants.DefaultMaximumDefenseCredits));
            this.Stats[CreatureStat.DefensePoints].ValueChanged += this.RaiseStatChange;

            this.StatChanged += (ICreature creature, IStat statThatChanged, uint previousValue, byte previousPercent) =>
            {
                if (statThatChanged.Type == CreatureStat.HitPoints && statThatChanged.Current == 0)
                {
                    this.Death?.Invoke(this);
                }
            };
        }

        /// <summary>
        /// Event to call when the combatant dies.
        /// </summary>
        public event CombatantDeathHandler Death;

        /// <summary>
        /// Event to call when the attack target changes.
        /// </summary>
        public event CombatantAttackTargetChangedHandler AttackTargetChanged;

        /// <summary>
        /// Event to call when the chase target changes.
        /// </summary>
        public event CombatantFollowTargetChangedHandler FollowTargetChanged;

        /// <summary>
        /// Event triggered when a skill of this creature changes.
        /// </summary>
        public event SkilledCreatureSkillChangedHandler SkillChanged;

        /// <summary>
        /// Event called when this creature senses another.
        /// </summary>
        public event SentientCreatureCreaturePerceivedHandler CreatureSensed;

        /// <summary>
        /// Event called when this creature sees another.
        /// </summary>
        public event SentientCreatureCreatureSeenHandler CreatureSeen;

        /// <summary>
        /// Event called when this creature loses track of a sensed creature.
        /// </summary>
        public event SentientCreatureCreatureLostHandler CreatureLost;

        /// <summary>
        /// Gets the creatures who are sensed by this creature.
        /// </summary>
        public IEnumerable<ICreature> PerceivedCreatures => this.creatureAwarenessMap.Keys;

        /// <summary>
        /// Gets or sets the target being chased, if any.
        /// </summary>
        public ICreature ChaseTarget { get; protected set; }

        /// <summary>
        /// Gets the current target combatant.
        /// </summary>
        public ICombatant AutoAttackTarget { get; private set; }

        /// <summary>
        /// Gets a metric of how fast a combatant can earn an attack credit per combat round.
        /// </summary>
        public decimal AttackSpeed => this.baseAttackSpeed + this.attackSpeedBuff;

        /// <summary>
        /// Gets a metric of how fast a combatant can earn a defense credit per combat round.
        /// </summary>
        public decimal DefenseSpeed => this.baseDefenseSpeed + this.defenseSpeedBuff;

        /// <summary>
        /// Gets or sets the fight mode selected by this combatant.
        /// </summary>
        public FightMode FightMode { get; set; }

        /// <summary>
        /// Gets or sets the chase mode selected by this combatant.
        /// </summary>
        public ChaseMode ChaseMode { get; set; }

        /// <summary>
        /// Gets the range that the auto attack has.
        /// </summary>
        public abstract byte AutoAttackRange { get; }

        /// <summary>
        /// Gets the distribution of damage taken by any combatant that has attacked this combatant while the current combat is active.
        /// </summary>
        public IEnumerable<(uint CombatantId, uint Damage)> DamageTakenInSession
        {
            get
            {
                return this.combatSessionDamageTakenMap.Select(kvp => (kvp.Key, kvp.Value)).ToList();
            }
        }

        /// <summary>
        /// Gets the collection of combatants currently attacking this combatant.
        /// </summary>
        public IEnumerable<ICombatant> AttackedBy
        {
            get
            {
                return this.combatSessionAttackedBy.ToList();
            }
        }

        /// <summary>
        /// Gets the current skills information for the combatant.
        /// </summary>
        /// <remarks>
        /// The key is a <see cref="SkillType"/>, and the value is a <see cref="ISkill"/>.
        /// </remarks>
        public IDictionary<SkillType, ISkill> Skills { get; }

        /// <summary>
        /// Sets the attack target of this combatant.
        /// </summary>
        /// <param name="otherCombatant">The other target combatant, if any.</param>
        /// <returns>True if the target was actually changed, false otherwise.</returns>
        public bool SetAttackTarget(ICombatant otherCombatant)
        {
            bool targetWasChanged = false;

            if (otherCombatant != this.AutoAttackTarget)
            {
                var oldTarget = this.AutoAttackTarget;

                this.AutoAttackTarget = otherCombatant;

                oldTarget?.UnsetAttackedBy(this);
                otherCombatant?.SetAttackedBy(this);

                if (this.ChaseMode != ChaseMode.Stand)
                {
                    this.SetFollowTarget(otherCombatant);
                }

                this.AttackTargetChanged?.Invoke(this, oldTarget);

                targetWasChanged = true;
            }

            return targetWasChanged;
        }

        /// <summary>
        /// Sets the chasing target of this combatant.
        /// </summary>
        /// <param name="target">The target to chase, if any.</param>
        /// <returns>True if the target was actually changed, false otherwise.</returns>
        public bool SetFollowTarget(ICreature target)
        {
            bool targetWasChanged = false;

            if (target != this.ChaseTarget)
            {
                var oldTarget = this.ChaseTarget;

                this.ChaseTarget = target;

                this.FollowTargetChanged?.Invoke(this, oldTarget);

                targetWasChanged = true;
            }

            return targetWasChanged;
        }

        /// <summary>
        /// Calculates the current percentual value between current and target counts for the given skill.
        /// </summary>
        /// <param name="skillType">The type of skill to calculate for.</param>
        /// <returns>A value between [0, 100] representing the current percentual value.</returns>
        public byte CalculateSkillPercent(SkillType skillType)
        {
            const int LowerBound = 0;
            const int UpperBound = 100;

            if (!this.Skills.ContainsKey(skillType))
            {
                return LowerBound;
            }

            var unadjustedPercent = Math.Max(LowerBound, Math.Min(this.Skills[skillType].CurrentCount / this.Skills[skillType].CountForNextLevel, UpperBound));

            return (byte)Math.Floor(unadjustedPercent);
        }

        /// <summary>
        /// Starts tracking another <see cref="ICombatant"/>.
        /// </summary>
        /// <param name="otherCombatant">The other combatant, now in view.</param>
        public abstract void AddToCombatList(ICombatant otherCombatant);

        /// <summary>
        /// Stops tracking another <see cref="ICombatant"/>.
        /// </summary>
        /// <param name="otherCombatant">The other combatant, now in view.</param>
        public abstract void RemoveFromCombatList(ICombatant otherCombatant);

        /// <summary>
        /// Sets this combatant as being attacked by another.
        /// </summary>
        /// <param name="combatant">The combatant attacking this one, if any.</param>
        public void SetAttackedBy(ICombatant combatant)
        {
            if (combatant == null)
            {
                return;
            }

            this.combatSessionAttackedBy.Add(combatant);
        }

        /// <summary>
        /// Unsets this combatant as being attacked by another.
        /// </summary>
        /// <param name="combatant">The combatant no longer attacking this one, if any.</param>
        public void UnsetAttackedBy(ICombatant combatant)
        {
            if (combatant == null)
            {
                return;
            }

            this.combatSessionAttackedBy.Remove(combatant);
        }

        /// <summary>
        /// Adds a value of experience to the combatant, which can be positive or negative.
        /// </summary>
        /// <param name="expToGiveOrTake">The experience value to give or take.</param>
        public void AddExperience(long expToGiveOrTake)
        {
            if (!this.Skills.ContainsKey(SkillType.Experience) || expToGiveOrTake == 0)
            {
                return;
            }

            if (expToGiveOrTake < 0)
            {
                throw new NotSupportedException($"Taking experience is not supported yet. Therefore, {nameof(expToGiveOrTake)} ({expToGiveOrTake}) must not be negative.");
            }

            this.Skills[SkillType.Experience].IncreaseCounter(expToGiveOrTake);
        }

        /// <summary>
        /// Applies damage to the combatant, which is expected to apply reductions and protections.
        /// </summary>
        /// <param name="damageInfo">The information of the damage to make, without reductions.</param>
        /// <param name="fromCombatantId">The combatant from which to track the damage, if any.</param>
        /// <returns>The information about the damage actually done.</returns>
        public DamageInfo ApplyDamage(DamageInfo damageInfo, uint fromCombatantId = 0)
        {
            this.ApplyDamageModifiers(ref damageInfo);

            var currentHitpoints = this.Stats[CreatureStat.HitPoints].Current;

            if (damageInfo.Damage != 0)
            {
                this.Stats[CreatureStat.HitPoints].Decrease(damageInfo.Damage);
            }

            if (damageInfo.Damage > 0)
            {
                damageInfo.Damage = Math.Min(damageInfo.Damage, (int)currentHitpoints);
                damageInfo.Blood = this.BloodType;
                damageInfo.Effect = this.BloodType switch
                {
                    BloodType.Bones => AnimatedEffect.XGray,
                    BloodType.Fire => AnimatedEffect.XBlood,
                    BloodType.Slime => AnimatedEffect.Poison,
                    _ => AnimatedEffect.XBlood,
                };
            }

            if (fromCombatantId > 0)
            {
                this.combatSessionDamageTakenMap.AddOrUpdate(fromCombatantId, (uint)damageInfo.Damage, (key, oldValue) => (uint)(oldValue + damageInfo.Damage));
            }

            return damageInfo;
        }

        /// <summary>
        /// Increases the attack speed of this combatant.
        /// </summary>
        /// <param name="increaseAmount">The amount by which to increase.</param>
        // TODO: this is just for testing purposes and should be removed.
        public void IncreaseAttackSpeed(decimal increaseAmount)
        {
            this.attackSpeedBuff = Math.Min(CombatConstants.MaximumCombatSpeedFactor - this.baseAttackSpeed, this.attackSpeedBuff + increaseAmount);
        }

        /// <summary>
        /// Decreases the attack speed of this combatant.
        /// </summary>
        /// <param name="decreaseAmount">The amount by which to decrease.</param>
        // TODO: this is just for testing purposes and should be removed.
        public void DecreaseAttackSpeed(decimal decreaseAmount)
        {
            this.attackSpeedBuff = Math.Max(0, this.attackSpeedBuff - decreaseAmount);
        }

        /// <summary>
        /// Increases the defense speed of this combatant.
        /// </summary>
        /// <param name="increaseAmount">The amount by which to increase.</param>
        // TODO: this is just for testing purposes and should be removed.
        public void IncreaseDefenseSpeed(decimal increaseAmount)
        {
            this.defenseSpeedBuff = Math.Min(CombatConstants.MaximumCombatSpeedFactor - this.baseDefenseSpeed, this.defenseSpeedBuff + increaseAmount);
        }

        /// <summary>
        /// Decreases the defense speed of this combatant.
        /// </summary>
        /// <param name="decreaseAmount">The amount by which to decrease.</param>
        // TODO: this is just for testing purposes and should be removed.
        public void DecreaseDefenseSpeed(decimal decreaseAmount)
        {
            this.defenseSpeedBuff = Math.Max(0, this.defenseSpeedBuff - decreaseAmount);
        }

        /// <summary>
        /// Flags a creature as being perceived.
        /// </summary>
        /// <param name="creature">The creature that is perceived.</param>
        public void PerceiveCreature(ICreature creature)
        {
            creature.ThrowIfNull(nameof(creature));

            // Do not track self.
            if (creature == this)
            {
                return;
            }

            // Add a creature and mark it as sensed only.
            if (this.creatureAwarenessMap.TryAdd(creature, AwarenessLevel.Sensed))
            {
                this.CreatureSensed?.Invoke(this, creature);
            }

            // check if we can actually see the creature, and mark it as such if so.
            if (this.creatureAwarenessMap[creature] == AwarenessLevel.Sensed && this.CanSee(creature))
            {
                this.creatureAwarenessMap[creature] = AwarenessLevel.Seen;

                this.CreatureSeen?.Invoke(this, creature);
            }
        }

        /// <summary>
        /// Flags a creature as no longer perceived.
        /// </summary>
        /// <param name="creature">The creature that was lost.</param>
        public void LostCreature(ICreature creature)
        {
            creature.ThrowIfNull(nameof(creature));

            // We don't track self anyways.
            if (creature == this)
            {
                return;
            }

            if (this.creatureAwarenessMap.Remove(creature))
            {
                this.CreatureLost?.Invoke(this, creature);
            }
        }

        /// <summary>
        /// Raises the <see cref="SkillChanged"/> event for this creature on the given skill.
        /// </summary>
        /// <param name="forSkill">The skill to advance.</param>
        /// <param name="previousLevel">The previous skill level.</param>
        protected void RaiseSkillChange(SkillType forSkill, uint previousLevel)
        {
            if (!this.Skills.ContainsKey(forSkill))
            {
                return;
            }

            this.SkillChanged?.Invoke(this, this.Skills[forSkill], previousLevel, this.Skills[forSkill].Percent, 0);
        }

        /// <summary>
        /// Raises the <see cref="SkillChanged"/> event for this creature on the given skill.
        /// </summary>
        /// <param name="forSkill">The skill to advance.</param>
        /// <param name="previousPercent">The previous percent of completion to next level.</param>
        protected void RaiseSkillChange(SkillType forSkill, byte previousPercent)
        {
            if (!this.Skills.ContainsKey(forSkill))
            {
                return;
            }

            this.SkillChanged?.Invoke(this, this.Skills[forSkill], this.Skills[forSkill].CurrentLevel, previousPercent, 0);
        }

        /// <summary>
        /// Raises the <see cref="SkillChanged"/> event for this creature on the given skill.
        /// </summary>
        /// <param name="forSkill">The skill to advance.</param>
        /// <param name="countDelta">Optional. The delta in the count for the skill. Not always sent.</param>
        protected void RaiseSkillChange(SkillType forSkill, long countDelta)
        {
            if (!this.Skills.ContainsKey(forSkill))
            {
                return;
            }

            this.SkillChanged?.Invoke(this, this.Skills[forSkill], this.Skills[forSkill].CurrentLevel, this.Skills[forSkill].Percent, countDelta);
        }

        /// <summary>
        /// Applies damage modifiers to the damage information provided.
        /// </summary>
        /// <param name="damageInfo">The damage information.</param>
        protected abstract void ApplyDamageModifiers(ref DamageInfo damageInfo);
    }
}
