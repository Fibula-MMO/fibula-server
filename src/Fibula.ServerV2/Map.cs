// -----------------------------------------------------------------
// <copyright file="Map.cs" company="2Dudes">
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Fibula.Definitions.Data.Structures;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Constants;
    using Fibula.ServerV2.Contracts.Delegates;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents the map for the game server.
    /// </summary>
    public sealed class Map : IMap
    {
        /// <summary>
        /// Holds the <see cref="ITile"/>s data based on <see cref="Location"/>.
        /// </summary>
        private readonly ConcurrentDictionary<Location, ITile> tiles;

        private readonly ISet<string> loadedHashes;

        /// <summary>
        /// The map loader that the map will use.
        /// </summary>
        private readonly IMapLoader loader;

        /// <summary>
        /// Stores the logger to use.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger to use.</param>
        /// <param name="mapLoader">The map loader to use to load this map.</param>
        public Map(ILogger<Map> logger, IMapLoader mapLoader)
        {
            logger.ThrowIfNull(nameof(logger));
            mapLoader.ThrowIfNull(nameof(mapLoader));

            this.logger = logger;
            this.loader = mapLoader;

            this.tiles = new ConcurrentDictionary<Location, ITile>();
            this.loadedHashes = new HashSet<string>();
        }

        /// <summary>
        /// Event invoked when a window of coordinates in the map is loaded.
        /// </summary>
        public event MapWindowLoadedHandler WindowLoaded;

        /// <summary>
        /// Attempts to get a <see cref="ITile"/> at a given <see cref="Location"/>, if any.
        /// </summary>
        /// <param name="location">The location to get the file from.</param>
        /// <returns>A reference to the <see cref="ITile"/> found, or null if none was found.</returns>
        public ITile this[Location location] => this.HasTileAt(location, out ITile tile) ? tile : null;

        /// <summary>
        /// Attempts to get a <see cref="ITile"/> at a given <see cref="Location"/>, if any.
        /// </summary>
        /// <param name="location">The location to get the file from.</param>
        /// <param name="tile">A reference to the <see cref="ITile"/> found, if any.</param>
        /// <param name="eagerLoad">Optional. A value indicating whether to attempt to load tiles if the loader hasn't loaded them yet.</param>
        /// <returns>A value indicating whether a <see cref="ITile"/> was found, false otherwise.</returns>
        public bool HasTileAt(Location location, out ITile tile, bool eagerLoad = true)
        {
            var mapWindow = new MapWindowDimensions(location.X, location.Y, location.Z);
            var hash = this.loader.GetLoadHash(mapWindow);

            if (eagerLoad && !this.loadedHashes.Contains(hash))
            {
                int minXLoaded = int.MaxValue;
                int maxXLoaded = int.MinValue;
                int minYLoaded = int.MaxValue;
                int maxYLoaded = int.MinValue;
                sbyte minZLoaded = sbyte.MaxValue;
                sbyte maxZLoaded = sbyte.MinValue;

                foreach (var t in this.loader.Load(mapWindow))
                {
                    if (t == null)
                    {
                        continue;
                    }

                    var loc = t.Location;

                    this.tiles[loc] = t;

                    // The tiles returned may not necessarily cover the same window supplied.
                    // This is because different loaders may choose to load a different area than specified.
                    // This is also why we use the hash logic above.
                    minXLoaded = Math.Min(loc.X, minXLoaded);
                    maxXLoaded = Math.Max(loc.X, maxXLoaded);

                    minYLoaded = Math.Min(loc.Y, minYLoaded);
                    maxYLoaded = Math.Max(loc.Y, maxYLoaded);

                    minZLoaded = Math.Min(loc.Z, minZLoaded);
                    maxZLoaded = Math.Max(loc.Z, maxZLoaded);
                }

                if (minXLoaded < int.MaxValue)
                {
                    this.loadedHashes.Add(hash);

                    this.logger.LogTrace($"Map window loaded: [{minXLoaded}->{maxXLoaded}, {minYLoaded}->{maxYLoaded}, {minZLoaded}->{maxZLoaded}].");

                    this.WindowLoaded?.Invoke(minXLoaded, maxXLoaded, minYLoaded, maxYLoaded, minZLoaded, maxZLoaded);
                }
            }

            return this.tiles.TryGetValue(location, out tile);
        }

        /// <summary>
        /// Gets the description tiles of the map on behalf of a given player at a given location.
        /// </summary>
        /// <param name="player">The player for which the description is being retrieved for.</param>
        /// <param name="centerLocation">The center location from which the description is being retrieved.</param>
        /// <returns>The description tiles representing the description.</returns>
        public IEnumerable<ITile> DescribeAt(IPlayer player, Location centerLocation)
        {
            player.ThrowIfNull(nameof(player));

            ushort fromX = (ushort)(centerLocation.X - 8);
            ushort fromY = (ushort)(centerLocation.Y - 6);

            sbyte fromZ = (sbyte)(centerLocation.Z > 7 ? centerLocation.Z - 2 : 7);
            sbyte toZ = (sbyte)(centerLocation.Z > 7 ? centerLocation.Z + 2 : 0);

            return this.DescribeWindow(player, fromX, fromY, fromZ, toZ);
        }

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
        /// <param name="startingOffsetZ">Optional. A starting offset for Z.</param>
        /// <returns>The description tiles representing the description.</returns>
        public IEnumerable<ITile> DescribeWindow(IPlayer player, ushort startX, ushort startY, sbyte startZ, sbyte endZ, byte windowSizeX = MapConstants.DefaultWindowSizeX, byte windowSizeY = MapConstants.DefaultWindowSizeY, sbyte startingOffsetZ = 0)
        {
            player.ThrowIfNull(nameof(player));

            ushort toX = (ushort)(startX + windowSizeX);
            ushort toY = (ushort)(startY + windowSizeY);

            var tiles = new List<ITile>();

            sbyte stepZ = 1;

            if (startZ > endZ)
            {
                // we're going up!
                stepZ = -1;
            }

            startingOffsetZ = startingOffsetZ != 0 ? startingOffsetZ : (sbyte)(player.Location.Z - startZ);

            if (windowSizeX > MapConstants.DefaultWindowSizeX)
            {
                this.logger.LogWarning($"{nameof(this.DescribeWindow)} {nameof(windowSizeX)} is over {nameof(MapConstants.DefaultWindowSizeX)} ({MapConstants.DefaultWindowSizeX}).");
            }

            if (windowSizeY > MapConstants.DefaultWindowSizeY)
            {
                this.logger.LogWarning($"{nameof(this.DescribeWindow)} {nameof(windowSizeY)} is over {nameof(MapConstants.DefaultWindowSizeY)} ({MapConstants.DefaultWindowSizeY}).");
            }

            var allCreatureIdsToLearn = new HashSet<uint>();
            var allCreatureIdsToForget = new HashSet<uint>();

            for (sbyte currentZ = startZ; currentZ != endZ + stepZ; currentZ += stepZ)
            {
                var zOffset = startZ - currentZ + startingOffsetZ;

                for (var nx = 0; nx < windowSizeX; nx++)
                {
                    for (var ny = 0; ny < windowSizeY; ny++)
                    {
                        Location targetLocation = new Location
                        {
                            X = (ushort)(startX + nx + zOffset),
                            Y = (ushort)(startY + ny + zOffset),
                            Z = currentZ,
                        };

                        tiles.Add(this[targetLocation]);
                    }
                }
            }

            return tiles;
        }
    }
}
