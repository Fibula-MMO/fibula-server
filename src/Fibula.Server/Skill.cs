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

namespace Fibula.Server.Creatures
{
    using System;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Delegates;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a creature's standard skill.
    /// </summary>
    public class Skill : ISkill
    {
        /// <summary>
        /// The value to use as the minimum percentual.
        /// Used to determine the <see cref="Percent"/> value.
        /// </summary>
        private const int MinPercentValue = 0;

        /// <summary>
        /// The value to use as the maximum percentual.
        /// Used to determine the <see cref="Percent"/> value.
        /// </summary>
        private const int MaxPercentValue = 100;

        /// <summary>
        /// The formula for this skill's progression.
        /// </summary>
        private readonly Func<ISkill, (double LowerCountBoundary, double UpperCountBoundary)> boundariesFormula;

        /// <summary>
        /// Initializes a new instance of the <see cref="Skill"/> class.
        /// </summary>
        /// <param name="type">This skill's type.</param>
        /// <param name="boundariesFormula">The formula used to determine the lower and upper boundaries for this skill.</param>
        /// <param name="defaultLevel">This skill's default level.</param>
        /// <param name="level">This skill's current level.</param>
        /// <param name="maxLevel">This skill's maximum level.</param>
        /// <param name="currentCount">This skill's current count.</param>
        public Skill(SkillType type, Func<ISkill, (double LowerCountBoundary, double UpperCountBoundary)> boundariesFormula, uint defaultLevel, uint level = 0, uint maxLevel = 1, double currentCount = 0)
        {
            boundariesFormula.ThrowIfNull(nameof(boundariesFormula));

            if (maxLevel == 0)
            {
                throw new ArgumentException($"{nameof(maxLevel)} must be positive.", nameof(maxLevel));
            }

            if (maxLevel < defaultLevel)
            {
                throw new ArgumentException($"{nameof(maxLevel)} must be at least the same value as {nameof(defaultLevel)}.", nameof(maxLevel));
            }

            if (currentCount < 0)
            {
                throw new ArgumentException($"{nameof(currentCount)} cannot be negative.", nameof(currentCount));
            }

            this.Type = type;
            this.DefaultLevel = defaultLevel;
            this.MaximumLevel = maxLevel;
            this.CurrentLevel = Math.Min(this.MaximumLevel, level == 0 ? defaultLevel : level);

            this.boundariesFormula = boundariesFormula;

            // Add the current count to the skill.
            // This will make sure we land on the right count boundaries.
            this.IncreaseCounter(currentCount);
        }

        /// <summary>
        /// Event triggered when this skill level changes.
        /// </summary>
        public event SkillLevelChangedHandler LevelChanged;

        /// <summary>
        /// Event triggered when this skill count changes.
        /// </summary>
        public event SkillCountChangedHandler CountChanged;

        /// <summary>
        /// Event triggered when this skill percent changes.
        /// </summary>
        public event SkillPercentChangedHandler PercentChanged;

        /// <summary>
        /// Gets this skill's type.
        /// </summary>
        public SkillType Type { get; }

        /// <summary>
        /// Gets this skill's level.
        /// </summary>
        public uint CurrentLevel { get; private set; }

        /// <summary>
        /// Gets this skill's maximum level.
        /// </summary>
        public uint MaximumLevel { get; }

        /// <summary>
        /// Gets this skill's default level.
        /// </summary>
        public uint DefaultLevel { get; }

        /// <summary>
        /// Gets the count at which the current level starts.
        /// </summary>
        public double CountAtStartOfLevel { get; private set; }

        /// <summary>
        /// Gets this skill's target count.
        /// </summary>
        public double CountForNextLevel { get; private set; }

        /// <summary>
        /// Gets this skill's current count.
        /// </summary>
        public double CurrentCount { get; private set; }

        /// <summary>
        /// Gets the current percentual value between current and target counts this skill.
        /// </summary>
        public byte Percent
        {
            get
            {
                var fromCount = Math.Max(MinPercentValue, this.CurrentCount - this.CountAtStartOfLevel);
                var toCount = Math.Max(1, this.CountForNextLevel - this.CountAtStartOfLevel);

                var unadjustedPercent = Math.Max(MinPercentValue, Math.Min(fromCount / toCount, MaxPercentValue)) * MaxPercentValue;

                return (byte)Math.Floor(unadjustedPercent);
            }
        }

        /// <summary>
        /// Increases this skill's counter.
        /// </summary>
        /// <param name="value">The amount by which to increase this skills counter.</param>
        public void IncreaseCounter(double value)
        {
            if (value <= 0)
            {
                return;
            }

            var lastCount = this.CurrentCount;
            var lastLevel = this.CurrentLevel;
            var lastPercentVal = this.Percent;

            if (this.CountForNextLevel == 0)
            {
                // If the target count is zero, it may not be initialized.
                var (lowerBoundary, higherBoundary) = this.boundariesFormula(this);

                // It may also be actually zero, so we'll just exit, because we don't want infinite level advances.
                if (higherBoundary == 0)
                {
                    return;
                }

                this.CountAtStartOfLevel = lowerBoundary;
                this.CountForNextLevel = higherBoundary;
            }

            this.CurrentCount = Math.Min(double.MaxValue, this.CurrentCount + value);

            // Skill level advance
            while (this.CurrentCount >= this.CountForNextLevel)
            {
                this.CurrentLevel++;

                var (lowerBoundary, higherBoundary) = this.boundariesFormula(this);

                if (this.CountForNextLevel == higherBoundary)
                {
                    // Prevent infinite level advances.
                    break;
                }

                this.CountAtStartOfLevel = lowerBoundary;
                this.CountForNextLevel = higherBoundary;
            }

            // Invoke any subscribers to the change events.
            if (lastCount != this.CurrentCount)
            {
                this.CountChanged?.Invoke(this.Type, (long)value);
            }

            if (this.CurrentLevel != lastLevel)
            {
                this.LevelChanged?.Invoke(this.Type, lastLevel);
            }

            if (this.Percent != lastPercentVal)
            {
                this.PercentChanged?.Invoke(this.Type, lastPercentVal);
            }
        }
    }
}
