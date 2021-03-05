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

namespace Fibula.Creatures
{
    using System;
    using Fibula.Client.Contracts.Abstractions;
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Common.Contracts.Constants;
    using Fibula.Creatures.Contracts.Abstractions;
    using Fibula.Creatures.Contracts.Enumerations;
    using Fibula.Data.Entities.Contracts.Abstractions;
    using Fibula.Definitions.Enumerations;
    using Fibula.Mechanics.Contracts.Abstractions;
    using Fibula.Mechanics.Contracts.Structs;
    using Fibula.Scripting.Formulae;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents all players in the game.
    /// </summary>
    public class Player : CombatantCreature, IPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="client">The client to associate this player to.</param>
        /// <param name="characterEntity">The player's corresponding character entity lodaded from storage.</param>
        public Player(
            IClient client,
            ICharacterEntity characterEntity)
            : base(characterEntity)
        {
            client.ThrowIfNull(nameof(client));
            characterEntity.ThrowIfNull(nameof(characterEntity));

            this.Client = client;
            this.Client.PlayerId = this.Id;

            this.CharacterId = characterEntity.Id;

            this.Outfit = characterEntity.Outfit;

            this.InitializeSkills();

            var expLevel = this.Skills.TryGetValue(SkillType.Experience, out ISkill expSkill) ? expSkill.CurrentLevel : 1;

            this.Stats[CreatureStat.BaseSpeed].Set(220 + (2 * (expLevel - 1)));
            this.Stats[CreatureStat.CarryStrength].Set(150);

            this.Stats.Add(CreatureStat.ManaPoints, new Stat(CreatureStat.ManaPoints, characterEntity.CurrentManapoints == default ? characterEntity.MaxHitpoints : characterEntity.CurrentManapoints, characterEntity.MaxManapoints));
            this.Stats[CreatureStat.ManaPoints].Changed += this.RaiseStatChange;

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
        /// Gets the client associated to this player.
        /// </summary>
        public IClient Client { get; }

        /// <summary>
        /// Gets this player's speed.
        /// </summary>
        public override ushort Speed
        {
            get => (ushort)(this.Stats[CreatureStat.BaseSpeed].Current + (2 * this.VariableSpeed));
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
