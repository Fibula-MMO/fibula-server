﻿// -----------------------------------------------------------------
// <copyright file="Stat.cs" company="2Dudes">
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
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Delegates;
    using Fibula.Server.Contracts.Enumerations;

    /// <summary>
    /// Class that represents a stat.
    /// </summary>
    public class Stat : IStat
    {
        /// <summary>
        /// Stores the current value of the stat.
        /// </summary>
        private long current;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stat"/> class.
        /// </summary>
        /// <param name="statType">The stat type.</param>
        /// <param name="currentValue">The current stat value.</param>
        /// <param name="maxValue">The maximum value that the stat can reach.</param>
        public Stat(CreatureStat statType, uint currentValue, uint maxValue)
        {
            this.Type = statType;
            this.Current = currentValue;
            this.Maximum = maxValue;
        }

        /// <summary>
        /// Event triggered when this stat value changes.
        /// </summary>
        public event StatValueChangedHandler ValueChanged;

        /// <summary>
        /// Event triggered when this stat percentual value changes.
        /// </summary>
        public event StatPercentChangedHandler PercentChanged;

        /// <summary>
        /// Gets this stat's id.
        /// </summary>
        public CreatureStat Type { get; }

        /// <summary>
        /// Gets this stat's current value.
        /// </summary>
        public uint Current
        {
            get => (uint)this.current;
            private set => this.current = value;
        }

        /// <summary>
        /// Gets this stat's maximum value.
        /// </summary>
        public uint Maximum { get; private set; }

        /// <summary>
        /// Gets the current percentual value between current and maximum values.
        /// </summary>
        public byte Percent => Convert.ToByte(this.Current == 0 ? 0x00 : Math.Min(100, Math.Max(1, this.Current * 100 / this.Maximum)));

        /// <summary>
        /// Increases this stats's value.
        /// </summary>
        /// <param name="value">The amount by which to increase this stat's value.</param>
        /// <returns>True if the value was actually increased, false otherwise.</returns>
        public bool Increase(int value)
        {
            // Set handles the maximum limit.
            return this.Set((uint)Math.Max(0, this.current + value));
        }

        /// <summary>
        /// Increases this stats's maximum value.
        /// </summary>
        /// <param name="value">The amount by which to increase this stat's maximum value.</param>
        /// <returns>True if the value was actually increased, false otherwise.</returns>
        public bool IncreaseMaximum(int value)
        {
            var lastMaximumVal = this.Maximum;

            this.Maximum = Math.Min(uint.MaxValue, (uint)Math.Max(0, this.Maximum + value));

            if (this.Maximum != lastMaximumVal)
            {
                // We'll need to increase the normal value too.
                return this.Increase(value);
            }

            return false;
        }

        /// <summary>
        /// Decreases this stats's value.
        /// </summary>
        /// <param name="value">The amount by which to decrease this stat's value.</param>
        /// <returns>True if the value was actually decreased, false otherwise.</returns>
        public bool Decrease(int value)
        {
            // Set handles the maximum limit.
            return this.Set((uint)Math.Max(0, this.current - value));
        }

        /// <summary>
        /// Decreases this stats's maximum value.
        /// </summary>
        /// <param name="value">The amount by which to decrease this stat's maximum value.</param>
        /// <returns>True if the value was actually decreased, false otherwise.</returns>
        public bool DecreaseMaximum(int value)
        {
            var lastMaximumVal = this.Maximum;

            this.Maximum = Math.Min(uint.MaxValue, (uint)Math.Max(0, this.Maximum - value));

            if (this.Maximum != lastMaximumVal)
            {
                // Cap the current to maximum if lower.
                return this.Set(Math.Min(this.Current, this.Maximum));
            }

            return false;
        }

        /// <summary>
        /// Sets this stats's value.
        /// </summary>
        /// <param name="value">The value to set in the stat. This is bounded by [0, <see cref="Maximum"/>].</param>
        /// <returns>True if the value was actually changed, false otherwise.</returns>
        public bool Set(uint value)
        {
            var lastVal = this.Current;
            var lastPercentVal = this.Percent;

            this.Current = Math.Min(this.Maximum, Math.Max(0, value));

            // Invoke any subscribers to the change events.
            if (this.Current != lastVal)
            {
                this.ValueChanged?.Invoke(this.Type, lastVal);
            }

            if (this.Percent != lastPercentVal)
            {
                this.PercentChanged?.Invoke(this.Type, lastPercentVal);
            }

            return this.Current != lastVal || this.Percent != lastPercentVal;
        }
    }
}
