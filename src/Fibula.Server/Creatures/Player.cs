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

namespace Fibula.Server.Creatures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fibula.Definitions.Constants;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Enumerations;
    using Fibula.Scripting.Formulae;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Contracts.Structs;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents all players in the game.
    /// </summary>
    public class Player : CombatantCreature, IPlayer
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

            this.InitializeSkills();
            this.InitializeStats(characterEntity.Stats);

            // Override some of the stats for now.
            var expLevel = this.Skills.TryGetValue(SkillType.Experience, out ISkill expSkill) ? expSkill.CurrentLevel : 1;

            this.Stats[CreatureStat.BaseSpeed].Set(220 + (2 * (expLevel - 1)));
            this.Stats[CreatureStat.CarryStrength].Set(150);

            this.Stats.Add(CreatureStat.SoulPoints, new Stat(CreatureStat.SoulPoints, 0, 0));
            this.Stats[CreatureStat.SoulPoints].Changed += this.RaiseStatChange;

            this.Inventory = new PlayerInventory(this);

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
        /// Gets a value indicating whether this player can be moved by others.
        /// </summary>
        public override bool CanBeMoved => this.PermissionsLevel == 0;

        /// <summary>
        /// Gets the player's soul points.
        /// </summary>
        // TODO: nobody likes soulpoints... figure out what to do with them.
        public byte SoulPoints { get; }

        /// <summary>
        /// Gets the player's profession.
        /// </summary>
        public ProfessionType Profession { get; }

        /// <summary>
        /// Gets or sets the inventory for the player.
        /// </summary>
        public sealed override IInventory Inventory { get; protected set; }

        /// <summary>
        /// Gets the range that the auto attack has.
        /// </summary>
        public override byte AutoAttackRange => 1;

        /// <summary>
        /// Gets this player's speed.
        /// </summary>
        public override ushort Speed
        {
            get => (ushort)(this.Stats[CreatureStat.BaseSpeed].Current + (2 * this.VariableSpeed));
        }

        /// <summary>
        /// Selects and returns a new in-game id for a player.
        /// </summary>
        /// <returns>The picked id.</returns>
        public static uint ReserveNewId()
        {
            return Creature.NewCreatureId();
        }

        /// <summary>
        /// Starts tracking another <see cref="ICombatant"/>.
        /// </summary>
        /// <param name="otherCombatant">The other combatant, now in view.</param>
        public override void AddToCombatList(ICombatant otherCombatant)
        {
            // Players don't just start attacking the new guy, so do nothing here.
        }

        /// <summary>
        /// Stops tracking another <see cref="ICombatant"/>.
        /// </summary>
        /// <param name="otherCombatant">The other combatant, now in view.</param>
        public override void RemoveFromCombatList(ICombatant otherCombatant)
        {
            if (otherCombatant == this.AutoAttackTarget)
            {
                this.SetAttackTarget(null);
            }
        }

        /// <summary>
        /// Creates a new <see cref="Player"/> that is a shallow copy of the current instance.
        /// </summary>
        /// <returns>A new <see cref="Player"/> that is a shallow copy of this instance.</returns>
        public override IThing Clone()
        {
            throw new NotSupportedException();
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

            if (damageInfo.Damage > 0 && damageInfo.Dealer != null && damageInfo.Dealer is IPlayer)
            {
                damageInfo.Damage = (int)Math.Ceiling((decimal)damageInfo.Damage / 2);
            }
        }

        private void InitializeStats(IEnumerable<CharacterStatEntity> stats)
        {
            if (stats == null || !stats.Any())
            {
                throw new ArgumentNullException(nameof(stats));
            }

            // TODO: move somewhere or merge
            static CreatureStat ToCreatureStat(CharacterStat stat)
            {
                return stat switch
                {
                    CharacterStat.HitPoints => CreatureStat.HitPoints,
                    CharacterStat.ManaPoints => CreatureStat.ManaPoints,
                    CharacterStat.BaseSpeed => CreatureStat.BaseSpeed,
                    CharacterStat.CarryStrength => CreatureStat.CarryStrength,
                    _ => throw new NotSupportedException($"Unsupported stat {stat}.")
                };
            }

            foreach (var stat in stats)
            {
                var creatureStat = ToCreatureStat(stat.Type);

                if (!this.Stats.ContainsKey(creatureStat))
                {
                    this.Stats.Add(creatureStat, new Stat(creatureStat, stat.Current == default ? stat.Maximum : stat.Current, stat.Maximum));
                    this.Stats[creatureStat].Changed += this.RaiseStatChange;
                }
                else
                {
                    // TODO: set maximum and minimum?
                    this.Stats[creatureStat].Set(stat.Current);
                }
            }
        }

        private void InitializeSkills()
        {
            (double LowerCountBoundary, double UpperCountBoundary) BoundariesFunc(ICreatureWithSkills creature, ISkill skill, Func<ISkillProgressionFormulaInput, double> formula)
            {
                var input = new SkillProgressionFormulaInput(skill.Type, skill.CurrentLevel, this.Profession);

                if (formula != null)
                {
                    var nextTargetCount = formula(input);

                    // The old count for next level is the new low boundary.
                    return (skill.CountForNextLevel, nextTargetCount);
                }

                return (0.00, 0.00);
            }

            this.Skills[SkillType.Experience] = new Skill(this, SkillType.Experience, (c, s) => BoundariesFunc(c, s, ConstantFormulae.ExperienceNextTargetCountDelegate), 1, maxLevel: 150, notifyOnEveryCounterChange: true);
            this.Skills[SkillType.Experience].Changed += this.RaiseSkillChange;

            this.Skills[SkillType.Magic] = new Skill(this, SkillType.Magic, (c, s) => BoundariesFunc(c, s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 0, maxLevel: 150);
            this.Skills[SkillType.Magic].Changed += this.RaiseSkillChange;

            this.Skills[SkillType.NoWeapon] = new Skill(this, SkillType.NoWeapon, (c, s) => BoundariesFunc(c, s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.NoWeapon].Changed += this.RaiseSkillChange;

            this.Skills[SkillType.Axe] = new Skill(this, SkillType.Axe, (c, s) => BoundariesFunc(c, s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.Axe].Changed += this.RaiseSkillChange;

            this.Skills[SkillType.Club] = new Skill(this, SkillType.Club, (c, s) => BoundariesFunc(c, s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.Club].Changed += this.RaiseSkillChange;

            this.Skills[SkillType.Sword] = new Skill(this, SkillType.Sword, (c, s) => BoundariesFunc(c, s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.Sword].Changed += this.RaiseSkillChange;

            this.Skills[SkillType.Shield] = new Skill(this, SkillType.Shield, (c, s) => BoundariesFunc(c, s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.Shield].Changed += this.RaiseSkillChange;

            this.Skills[SkillType.Ranged] = new Skill(this, SkillType.Ranged, (c, s) => BoundariesFunc(c, s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.Ranged].Changed += this.RaiseSkillChange;

            this.Skills[SkillType.Fishing] = new Skill(this, SkillType.Fishing, (c, s) => BoundariesFunc(c, s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.Fishing].Changed += this.RaiseSkillChange;
        }
    }
}
