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
    using Fibula.Server.Contracts.Structures;
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
            this.Stats[CreatureStat.SoulPoints].ValueChanged += this.RaiseStatChange;

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
            get => (ushort)(this.Stats[CreatureStat.BaseSpeed].Current + (2 * this.VariableSpeed));
        }

        /// <summary>
        /// Gets the range that the auto attack has.
        /// </summary>
        public override byte AutoAttackRange => 1;  // TODO: inventory items will modify this.

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
                    this.Stats[creatureStat].ValueChanged += this.RaiseStatChange;
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
            (double LowerCountBoundary, double UpperCountBoundary) BoundariesFunc(ISkill skill, Func<ISkillProgressionFormulaInput, double> formula)
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

            this.Skills[SkillType.Experience] = new Skill(SkillType.Experience, (s) => BoundariesFunc(s, ConstantFormulae.ExperienceNextTargetCountDelegate), 1, maxLevel: 150);
            this.Skills[SkillType.Experience].CountChanged += this.RaiseSkillChange;
            this.Skills[SkillType.Experience].LevelChanged += this.RaiseSkillChange;
            this.Skills[SkillType.Experience].PercentChanged += this.RaiseSkillChange;

            this.Skills[SkillType.Magic] = new Skill(SkillType.Magic, (s) => BoundariesFunc(s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 0, maxLevel: 150);
            this.Skills[SkillType.Magic].LevelChanged += this.RaiseSkillChange;
            this.Skills[SkillType.Magic].PercentChanged += this.RaiseSkillChange;

            this.Skills[SkillType.NoWeapon] = new Skill(SkillType.NoWeapon, (s) => BoundariesFunc(s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.NoWeapon].LevelChanged += this.RaiseSkillChange;
            this.Skills[SkillType.NoWeapon].PercentChanged += this.RaiseSkillChange;

            this.Skills[SkillType.Axe] = new Skill(SkillType.Axe, (s) => BoundariesFunc(s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.Axe].LevelChanged += this.RaiseSkillChange;
            this.Skills[SkillType.Axe].PercentChanged += this.RaiseSkillChange;

            this.Skills[SkillType.Club] = new Skill(SkillType.Club, (s) => BoundariesFunc(s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.Club].LevelChanged += this.RaiseSkillChange;
            this.Skills[SkillType.Club].PercentChanged += this.RaiseSkillChange;

            this.Skills[SkillType.Sword] = new Skill(SkillType.Sword, (s) => BoundariesFunc(s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.Sword].LevelChanged += this.RaiseSkillChange;
            this.Skills[SkillType.Sword].PercentChanged += this.RaiseSkillChange;

            this.Skills[SkillType.Shield] = new Skill(SkillType.Shield, (s) => BoundariesFunc(s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.Shield].LevelChanged += this.RaiseSkillChange;
            this.Skills[SkillType.Shield].PercentChanged += this.RaiseSkillChange;

            this.Skills[SkillType.Ranged] = new Skill(SkillType.Ranged, (s) => BoundariesFunc(s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.Ranged].LevelChanged += this.RaiseSkillChange;
            this.Skills[SkillType.Ranged].PercentChanged += this.RaiseSkillChange;

            this.Skills[SkillType.Fishing] = new Skill(SkillType.Fishing, (s) => BoundariesFunc(s, ConstantFormulae.DefaultSkillNextTargetCountDelegate), 10, maxLevel: 150);
            this.Skills[SkillType.Fishing].LevelChanged += this.RaiseSkillChange;
            this.Skills[SkillType.Fishing].PercentChanged += this.RaiseSkillChange;
        }
    }
}
