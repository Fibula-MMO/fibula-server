// -----------------------------------------------------------------
// <copyright file="Thing.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Fibula.Definitions.Data.Structures;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Delegates;

    /// <summary>
    /// Class that represents all things in the game.
    /// </summary>
    public abstract class Thing : IThing
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Thing"/> class.
        /// </summary>
        public Thing()
        {
            this.UniqueId = Guid.NewGuid();
        }

        /// <summary>
        /// Event to invoke when the location of this thing has changed.
        /// </summary>
        public event ThingLocationChangedHandler LocationChanged;

        /// <summary>
        /// Gets the id of the type of this thing.
        /// </summary>
        public abstract ushort TypeId { get; }

        /// <summary>
        /// Gets the unique id given to this thing.
        /// </summary>
        public Guid UniqueId { get; }

        /// <summary>
        /// Gets this thing's location.
        /// </summary>
        public abstract Location Location { get; }

        /// <summary>
        /// Gets the location where this thing is being carried at, if any.
        /// </summary>
        public abstract Location? CarryLocation { get; }

        /// <summary>
        /// Invokes the <see cref="LocationChanged"/> event on this thing.
        /// </summary>
        /// <param name="fromLocation">The location from which the change happened.</param>
        public void RaiseLocationChanged(Location fromLocation)
        {
            this.LocationChanged?.Invoke(this, fromLocation);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.DescribeForLogger();
        }

        /// <summary>
        /// Provides a string describing the current thing for logging purposes.
        /// </summary>
        /// <returns>The string to log.</returns>
        public abstract string DescribeForLogger();

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">The other object to compare against.</param>
        /// <returns>True if the current object is equal to the other parameter, false otherwise.</returns>
        public bool Equals([AllowNull] IThing other)
        {
            return this.UniqueId == other?.UniqueId;
        }
    }
}
