﻿// -----------------------------------------------------------------
// <copyright file="IThing.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Abstractions
{
    using System;
    using System.Collections.Generic;
    using Fibula.Scheduling.Contracts.Abstractions;
    using Fibula.Server.Contracts.Delegates;

    /// <summary>
    /// Interface for all things in the game.
    /// </summary>
    public interface IThing : ILocatable, IEquatable<IThing>
    {
        /// <summary>
        /// Event to invoke when the location of this thing has changed.
        /// </summary>
        event ThingLocationChangedHandler LocationChanged;

        /// <summary>
        /// Gets the id of this thing.
        /// </summary>
        ushort TypeId { get; }

        /// <summary>
        /// Gets the unique id of this thing.
        /// </summary>
        Guid UniqueId { get; }

        /// <summary>
        /// Gets the tracked events for this thing.
        /// </summary>
        IDictionary<string, IEvent> TrackedEvents { get; }

        /// <summary>
        /// Provides a string describing the current thing for logging purposes.
        /// </summary>
        /// <returns>The string to log.</returns>
        string DescribeForLogger();

        /// <summary>
        /// Makes the thing start tracking an event.
        /// </summary>
        /// <param name="evt">The event to stop tracking.</param>
        /// <param name="eventName">Optional. The name under which to start tracking the event. If no identifier is provided, the event's type name is used.</param>
        void StartTrackingEvent(IEvent evt, string eventName = "");

        /// <summary>
        /// Makes the thing stop tracking an event.
        /// </summary>
        /// <param name="evt">The event to stop tracking.</param>
        /// <param name="eventName">Optional. The name under which to look for and stop tracking the event. If no identifier is provided, the event's type name is used.</param>
        void StopTrackingEvent(IEvent evt, string eventName = "");
    }
}
