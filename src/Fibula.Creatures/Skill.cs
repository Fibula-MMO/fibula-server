// -----------------------------------------------------------------
// <copyright file="Skill.cs" company="2Dudes">
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
    using Fibula.Data.Entities.Contracts.Enumerations;
    using Fibula.Mechanics.Contracts.Abstractions;
    using Fibula.Mechanics.Contracts.Delegates;

    /// <summary>
    /// Class that represents a creature's standard skill.
    /// </summary>
    public class Skill : ISkill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Skill"/> class.
        /// </summary>
        /// <param name="type">This skill's type.</param>
        /// <param name="defaultLevel">This skill's default level.</param>
        /// <param name="rate">This skill's rate of target count increase.</param>
        /// <param name="baseIncrease">This skill's target base increase level over level.</param>
        /// <param name="level">This skill's current level.</param>
        /// <param name="maxLevel">This skill's maximum level.</param>
        /// <param name="count">This skill's current count.</param>
        /// <param name="notifyOnEveryCounterChange">Optional. A value indicating whether the skill shoudl raise changed events on every counter change.</param>
        public Skill(SkillType type, uint defaultLevel, double rate, double baseIncrease, uint level = 0, uint maxLevel = 1, double count = 0, bool notifyOnEveryCounterChange = false)
        {
            if (defaultLevel < 0)
            {
                throw new ArgumentException($"{nameof(defaultLevel)} must not be negative.", nameof(defaultLevel));
            }

            if (maxLevel < 1)
            {
                throw new ArgumentException($"{nameof(maxLevel)} must be positive.", nameof(maxLevel));
            }

            if (rate < 1)
            {
                throw new ArgumentException($"{nameof(rate)} must be positive.", nameof(rate));
            }

            if (baseIncrease < 1)
            {
                throw new ArgumentException($"{nameof(baseIncrease)} must be positive.", nameof(baseIncrease));
            }

            if (count < 0)
            {
                throw new ArgumentException($"{nameof(count)} cannot be negative.", nameof(count));
            }

            if (maxLevel < defaultLevel)
            {
                throw new ArgumentException($"{nameof(maxLevel)} must be at least the same value as {nameof(defaultLevel)}.", nameof(maxLevel));
            }

            this.Type = type;
            this.DefaultLevel = defaultLevel;
            this.MaxLevel = maxLevel;
            this.Level = Math.Min(this.MaxLevel, level == 0 ? defaultLevel : level);
            this.Rate = rate;
            this.BaseTargetIncrease = baseIncrease;

            // Add the current count to the skill.
            // This will make sure we land on the right count boundaries.
            this.IncreaseCounter(count);

            this.SendOnCounterChange = notifyOnEveryCounterChange;
        }

        /// <summary>
        /// Event triggered when this skill changed.
        /// </summary>
        public event OnSkillChanged Changed;

        /// <summary>
        /// Gets this skill's type.
        /// </summary>
        public SkillType Type { get; }

        /// <summary>
        /// Gets this skill's level.
        /// </summary>
        public uint Level { get; private set; }

        /// <summary>
        /// Gets this skill's maximum level.
        /// </summary>
        public uint MaxLevel { get; }

        /// <summary>
        /// Gets this skill's default level.
        /// </summary>
        public uint DefaultLevel { get; }

        /// <summary>
        /// Gets this skill's current count.
        /// </summary>
        public double Count { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this skill will raise <see cref="Changed"/> events every time the counter changes.
        /// </summary>
        public bool SendOnCounterChange { get; }

        /// <summary>
        /// Gets this skill's rate of target count increase.
        /// </summary>
        public double Rate { get; }

        /// <summary>
        /// Gets the count at which the current level starts.
        /// </summary>
        public double StartingCount { get; private set; }

        /// <summary>
        /// Gets this skill's target count.
        /// </summary>
        public double TargetCount { get; private set; }

        /// <summary>
        /// Gets this skill's target base increase level over level.
        /// </summary>
        public double BaseTargetIncrease { get; }

        /// <summary>
        /// Gets the current percentual value between current and target counts this skill.
        /// </summary>
        public byte Percent
        {
            get
            {
                var fromCount = Math.Max(0, this.Count - this.StartingCount);
                var toCount = Math.Max(1, this.TargetCount - this.StartingCount);

                var unadjustedPercent = Math.Max(0, Math.Min(fromCount / toCount, 100)) * 100;

                return (byte)Math.Floor(unadjustedPercent);
            }
        }

        /// <summary>
        /// Increases this skill's counter.
        /// </summary>
        /// <param name="value">The amount by which to increase this skills counter.</param>
        public void IncreaseCounter(double value)
        {
            if (value == 0)
            {
                return;
            }

            var lastLevel = this.Level;
            var lastPercentVal = this.Percent;

            if (this.TargetCount == 0)
            {
                // If the target count is zero, it may not be initialized.
                this.ResetCountBoundaries();

                // It may also be actually zero, so we'll just exit, because we don't want infinite level advances.
                if (this.TargetCount == 0)
                {
                    return;
                }
            }

            this.Count = Math.Min(this.TargetCount, this.Count + value);

            // Skill level advance
            if (Math.Abs(this.Count - this.TargetCount) < 0.001)
            {
                this.Level++;

                this.ResetCountBoundaries();
            }

            // Invoke any subscribers to the change event.
            if (this.SendOnCounterChange || this.Level != lastLevel || this.Percent != lastPercentVal)
            {
                this.Changed?.Invoke(this.Type, lastLevel, lastPercentVal, (long)value);
            }
        }

        /// <summary>
        /// Calculates and re-sets the starting and next target count boundaries.
        /// </summary>
        private void ResetCountBoundaries()
        {
            this.TargetCount = (this.TargetCount * this.Rate) + this.BaseTargetIncrease;

            for (int i = 0; i < Math.Max(0, this.Level - this.DefaultLevel); i++)
            {
                this.StartingCount = this.TargetCount;
                this.TargetCount = (this.TargetCount * this.Rate) + this.BaseTargetIncrease;
            }
        }
    }
}
