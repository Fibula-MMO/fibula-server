// -----------------------------------------------------------------
// <copyright file="IMap.cs" company="2Dudes">
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
    using System.Collections.Generic;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Server.Contracts.Constants;
    using Fibula.Server.Contracts.Delegates;

    /// <summary>
    /// Interface for the map in the game world.
    /// </summary>
    public interface IMap
    {
        /// <summary>
        /// Event invoked when a window of coordinates in the map is loaded.
        /// </summary>
        event MapWindowLoadedHandler WindowLoaded;

        /// <summary>
        /// Attempts to get a <see cref="ITile"/> at a given <see cref="Location"/>, if any.
        /// </summary>
        /// <param name="location">The location to get the file from.</param>
        /// <returns>A reference to the <see cref="ITile"/> found, or null if none was found.</returns>
        ITile this[Location location] { get; }

        /// <summary>
        /// Evaluates wheter the map has a <see cref="ITile"/> at a given <see cref="Location"/>.
        /// </summary>
        /// <param name="location">The location to get the file from.</param>
        /// <param name="tile">A reference to the <see cref="ITile"/> at the specified location, if any.</param>
        /// <param name="loadAsNeeded">Optional. A value indicating whether to attempt to load tiles if the loader hasn't loaded them yet.</param>
        /// <returns>A value indicating whether a <see cref="ITile"/> was found at the location false otherwise.</returns>
        bool HasTileAt(Location location, out ITile tile, bool loadAsNeeded = true);

        /// <summary>
        /// Gets the description tiles of the map on behalf of a given player at a given location.
        /// </summary>
        /// <param name="player">The player for which the description is being retrieved for.</param>
        /// <param name="location">The center location from which the description is being retrieved.</param>
        /// <returns>The description tiles representing the description.</returns>
        IEnumerable<ITile> DescribeAt(IPlayer player, Location location);

        /// <summary>
        /// Gets the description tiles of the map on behalf of a given player for the specified window.
        /// </summary>
        /// <param name="player">The player for which the description is being retrieved for.</param>
        /// <param name="startX">The starting X coordinate of the window.</param>
        /// <param name="startY">The starting Y coordinate of the window.</param>
        /// <param name="startZ">The starting Z coordinate of the window.</param>
        /// <param name="endZ">The ending Z coordinate of the window.</param>
        /// <param name="windowSizeX">The size of the window in X.</param>
        /// <param name="windowSizeY">The size of the window in Y.</param>
        /// <param name="startingZOffset">Optional. A starting offset for Z.</param>
        /// <returns>The description tiles representing the description.</returns>
        IEnumerable<ITile> DescribeWindow(IPlayer player, ushort startX, ushort startY, sbyte startZ, sbyte endZ, byte windowSizeX = MapConstants.DefaultWindowSizeX, byte windowSizeY = MapConstants.DefaultWindowSizeY, sbyte startingZOffset = 0);
    }
}
