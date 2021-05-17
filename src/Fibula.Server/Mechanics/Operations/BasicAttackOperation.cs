// -----------------------------------------------------------------
// <copyright file="BasicAttackOperation.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Mechanics.Operations
{
    using System;
    using Fibula.Definitions.Enumerations;
    using Fibula.Definitions.Flags;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Constants;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Contracts.Extensions;
    using Fibula.Server.Contracts.Structs;
    using Fibula.Server.Mechanics.Conditions;
    using Fibula.Server.Notifications;
    using Fibula.Utilities.Common.Extensions;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents the basic attack operation.
    /// </summary>
    public class BasicAttackOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicAttackOperation"/> class.
        /// </summary>
        /// <param name="attacker">The combatant that is attacking.</param>
        /// <param name="target">The combatant that is the target.</param>
        /// <param name="exhaustionCost">Optional. The exhaustion cost of this operation.</param>
        public BasicAttackOperation(ICombatant attacker, ICombatant target, TimeSpan exhaustionCost)
            : base(attacker?.Id ?? 0)
        {
            attacker.ThrowIfNull(nameof(attacker));
            target.ThrowIfNull(nameof(target));

            this.Target = target;
            this.Attacker = attacker;

            this.ExhaustionInfo[ExhaustionFlag.Combat] = exhaustionCost;

            this.TargetIdAtScheduleTime = attacker?.AutoAttackTarget?.Id ?? 0;
        }

        /// <summary>
        /// Gets the combatant that is attacking on this operation.
        /// </summary>
        public ICombatant Attacker { get; }

        /// <summary>
        /// Gets the combatant that is the target on this operation.
        /// </summary>
        public ICombatant Target { get; }

        /// <summary>
        /// Gets the id of the target at schedule time.
        /// </summary>
        public uint TargetIdAtScheduleTime { get; }

        /// <summary>
        /// Gets the absolute minimum damage that the combat operation can result in.
        /// </summary>
        public int MinimumDamage => 0;

        /// <summary>
        /// Gets the absolute maximum damage that the combat operation can result in.
        /// </summary>
        public int MaximumDamage { get; }

        /// <summary>
        /// Executes the operation's logic.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        protected override void Execute(IOperationContext context)
        {
            // We should stop any pending attack operation before carrying this one out.
            if (this.Attacker.TryRetrieveTrackedOperation(nameof(BasicAttackOperation), out IOperation attackersAtkOp) && attackersAtkOp != this)
            {
                // Cancel it first, and remove it.
                attackersAtkOp.Cancel();
            }

            var distanceBetweenCombatants = (this.Attacker?.Location ?? this.Target.Location) - this.Target.Location;

            // Pre-checks.
            var nullAttacker = this.Attacker == null;
            var isTargetAlreadyDead = this.Target.IsDead;
            var isCorrectTarget = nullAttacker || this.Attacker?.AutoAttackTarget?.Id == this.TargetIdAtScheduleTime;
            var enoughCredits = nullAttacker || this.Attacker?.Stats[CreatureStat.AttackPoints].Current > 0;
            var inRange = nullAttacker || (distanceBetweenCombatants.MaxValueIn2D <= this.Attacker.AutoAttackRange && distanceBetweenCombatants.Z == 0);
            var atIdealDistance = nullAttacker || distanceBetweenCombatants.MaxValueIn2D == this.Attacker.AutoAttackRange;
            var attackerIsMonster = !nullAttacker && this.Attacker is IMonster;
            var canAttackFromThere = nullAttacker || (inRange && context.Map.CanThrowBetweenLocations(this.Attacker.Location, this.Target.Location));

            var attackPerformed = false;

            try
            {
                if (!isCorrectTarget || isTargetAlreadyDead)
                {
                    // We're not attacking the correct target or it's already dead, so stop right here.
                    return;
                }

                if (!canAttackFromThere)
                {
                    if (!nullAttacker)
                    {
                        // Set the pending attack operation as this operation.
                        this.Attacker.StartTrackingEvent(this);

                        // And set this operation to repeat after some time, so that it can actually be expedited (or else it's just processed as usual).
                        this.RepeatAfter = TimeSpan.FromMilliseconds((int)Math.Ceiling(CombatConstants.DefaultCombatRoundTimeInMs / this.Attacker.AttackSpeed));
                    }

                    return;
                }

                if (!enoughCredits)
                {
                    return;
                }

                this.PerformAttack(context);

                attackPerformed = true;
            }
            finally
            {
                if (!attackPerformed)
                {
                    // Update the actual cost if the attack wasn't performed.
                    this.ExhaustionInfo.Remove(ExhaustionFlag.Combat);
                }
            }
        }

        /// <summary>
        /// Performs the auto attack from the <see cref="Attacker"/> to it's <see cref="Target"/>.
        /// </summary>
        /// <param name="context">The context of the operation.</param>
        private void PerformAttack(IOperationContext context)
        {
            // TODO: this method has grown pretty big, it should at least be broken down when formulas get implemented.
            var rng = new Random();

            // Calculate the damage to inflict without any protections and reductions,
            // i.e. the amount of damage that the attacker can generate as it is.
            var attackPower = 1 + rng.Next(this.Attacker == null ? 10 : (int)this.Attacker.Skills[SkillType.NoWeapon].CurrentLevel);

            var damageToApplyInfo = new DamageInfo(attackPower, this.Attacker);
            var damageDoneInfo = this.Target.ApplyDamage(damageToApplyInfo, this.Attacker?.Id ?? 0);
            var distanceOfAttack = (this.Target.Location - (this.Attacker?.Location ?? this.Target.Location)).MaxValueIn2D;

            this.SendNotification(context, new MagicEffectNotification(() => context.Map.FindPlayersThatCanSee(this.Target.Location), this.Target.Location, damageDoneInfo.Effect));

            if (damageDoneInfo.Damage != 0)
            {
                TextColor damageTextColor = damageDoneInfo.Blood switch
                {
                    BloodType.Bones => TextColor.LightGrey,
                    BloodType.Fire => TextColor.Orange,
                    BloodType.Slime => TextColor.Green,
                    _ => TextColor.Red,
                };

                if (damageDoneInfo.Damage < 0)
                {
                    damageTextColor = TextColor.LightBlue;
                }
                else if (this.Target is IPlayer tgtPlayer)
                {
                    var hitpointsLostMessage = $"You lose {damageDoneInfo.Damage} hitpoints";

                    if (damageDoneInfo.Dealer != null)
                    {
                        var name = string.IsNullOrWhiteSpace(damageDoneInfo.Dealer.Article) ? damageDoneInfo.Dealer.Name : $"{damageDoneInfo.Dealer.Article} {damageDoneInfo.Dealer.Name}";

                        hitpointsLostMessage += $" due to an attack by {name}";
                    }

                    hitpointsLostMessage += ".";

                    this.SendNotification(context, new TextMessageNotification(() => tgtPlayer.YieldSingleItem(), MessageType.StatusDefault, hitpointsLostMessage));
                }

                this.SendNotification(context, new AnimatedTextNotification(() => context.Map.FindPlayersThatCanSee(this.Target.Location), this.Target.Location, damageTextColor, Math.Abs(damageDoneInfo.Damage).ToString()));
            }

            if (distanceOfAttack > 1)
            {
                // TODO: actual projectile value.
                this.SendNotification(context, new ProjectileNotification(() => context.Map.FindPlayersThatCanSee(this.Target.Location), this.Attacker.Location, this.Target.Location, ProjectileType.Bolt));
            }

            if (this.Target.Stats[CreatureStat.DefensePoints].Decrease(1))
            {
                this.Target.Skills[SkillType.Shield].IncreaseCounter(1);

                // Restore the target's defense point.
                context.Scheduler.ScheduleEvent(
                    new StatChangeOperation(this.Target, CreatureStat.DefensePoints),
                    TimeSpan.FromMilliseconds((int)Math.Round(CombatConstants.DefaultCombatRoundTimeInMs / this.Target.DefenseSpeed)));
            }

            if (damageDoneInfo.ApplyBloodToEnvironment)
            {
                context.GameApi.CreateItemAtLocationAsync(this.Target.Location, context.PredefinedItemSet.FindSplatterForBloodType(this.Target.BloodType));
            }

            if (this.Attacker != null)
            {
                if (this.Attacker.Stats[CreatureStat.AttackPoints].Decrease(1))
                {
                    // TODO: increase the actual skill.
                    this.Attacker.Skills[SkillType.NoWeapon].IncreaseCounter(1);

                    // Restore the attacker's attack point.
                    context.Scheduler.ScheduleEvent(
                        new StatChangeOperation(this.Attacker, CreatureStat.AttackPoints),
                        TimeSpan.FromMilliseconds((int)Math.Round(CombatConstants.DefaultCombatRoundTimeInMs / this.Attacker.AttackSpeed)));
                }

                if (this.Attacker.Location != this.Target.Location && this.Attacker.Id != this.Target.Id)
                {
                    var directionToTarget = this.Attacker.Location.DirectionTo(this.Target.Location);
                    var turnToDirOp = new TurnToDirectionOperation(this.Attacker, directionToTarget);

                    context.Scheduler.ScheduleEvent(turnToDirOp);
                }

                if (this.Attacker is IPlayer attackerPlayer)
                {
                    context.GameApi.AddOrAggregateCondition(
                        attackerPlayer,
                        new InFightCondition(attackerPlayer),
                        duration: TimeSpan.FromMilliseconds(CombatConstants.DefaultInFightTimeInMs));
                }
            }

            if (this.Target is IPlayer targetPlayer)
            {
                if (this.Attacker != null)
                {
                    this.SendNotification(context, new SquareNotification(() => targetPlayer.YieldSingleItem(), this.Attacker.Id, SquareColor.Black));
                }

                context.GameApi.AddOrAggregateCondition(
                    targetPlayer,
                    new InFightCondition(targetPlayer),
                    duration: TimeSpan.FromMilliseconds(CombatConstants.DefaultInFightTimeInMs));
            }
        }
    }
}
