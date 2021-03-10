// -----------------------------------------------------------------
// <copyright file="MapExtensions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Map.Contracts.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Fibula.Common.Contracts.Structs;
    using Fibula.Creatures.Contracts.Abstractions;
    using Fibula.Map.Contracts.Abstractions;
    using Fibula.Map.Contracts.Constants;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Helper class that provides extensions for the <see cref="IMap"/> implementations.
    /// </summary>
    public static class MapExtensions
    {
        /// <summary>
        /// Gets the ids of any creatures that can sense the given location.
        /// </summary>
        /// <param name="map">A reference to the map.</param>
        /// <param name="location">The location to check if creatures can sense.</param>
        /// <param name="viewRangeX">The range in X to scan.</param>
        /// <param name="viewRangeY">The range in Y to scan.</param>
        /// <returns>A collection of connections.</returns>
        public static IEnumerable<ICreature> FindCreaturesThatCanSense(this IMap map, Location location, byte viewRangeX = MapConstants.DefaultWindowSizeX / 2, byte viewRangeY = MapConstants.DefaultWindowSizeY / 2)
        {
            map.ThrowIfNull(nameof(map));

            var creatures = new HashSet<ICreature>();

            var xWindowStart = location.X - viewRangeX;
            var xWindowEnd = location.X + viewRangeX;
            var yWindowStart = location.Y - viewRangeY;
            var yWindowEnd = location.Y + viewRangeY;
            var zWindowStart = (sbyte)(location.Z <= 7 ? 0 : location.Z - 2);
            var zWindowEnd = (sbyte)(location.Z <= 7 ? 8 : location.Z + 2);

            for (int x = xWindowStart; x <= xWindowEnd; x++)
            {
                for (int y = yWindowStart; y <= yWindowEnd; y++)
                {
                    for (sbyte z = zWindowStart; z <= zWindowEnd; z++)
                    {
                        var loc = new Location() { X = x, Y = y, Z = z };

                        if (!map.GetTileAt(loc, out ITile tile, loadAsNeeded: false))
                        {
                            continue;
                        }

                        foreach (var creature in tile.Creatures)
                        {
                            creatures.Add(creature);
                        }
                    }
                }
            }

            foreach (var creature in creatures)
            {
                if (creature == null || creature is IPlayer)
                {
                    continue;
                }

                yield return creature;
            }
        }

        /// <summary>
        /// Gets the ids of any creatures that can see the given locations.
        /// </summary>
        /// <param name="map">A reference to the map.</param>
        /// <param name="locations">The locations to check if players can see.</param>
        /// <returns>A collection of connections.</returns>
        public static IEnumerable<ICreature> FindCreaturesThatCanSee(this IMap map, params Location[] locations)
        {
            map.ThrowIfNull(nameof(map));

            var creatures = new HashSet<ICreature>();

            foreach (var location in locations)
            {
                var xWindowStart = location.X - (MapConstants.DefaultWindowSizeX / 2);
                var xWindowEnd = location.X + (MapConstants.DefaultWindowSizeX / 2);
                var yWindowStart = location.Y - (MapConstants.DefaultWindowSizeY / 2);
                var yWindowEnd = location.Y + (MapConstants.DefaultWindowSizeY / 2);

                for (int x = xWindowStart; x <= xWindowEnd; x++)
                {
                    for (int y = yWindowStart; y <= yWindowEnd; y++)
                    {
                        var loc = new Location() { X = x, Y = y, Z = location.Z };

                        if (!map.GetTileAt(loc, out ITile tile, loadAsNeeded: false))
                        {
                            continue;
                        }

                        foreach (var creature in tile.Creatures)
                        {
                            creatures.Add(creature);
                        }
                    }
                }
            }

            foreach (var creature in creatures)
            {
                if (creature == null || !locations.Any(loc => creature.CanSee(loc)))
                {
                    continue;
                }

                yield return creature;
            }
        }

        /// <summary>
        /// Gets the ids of any players that can see the given locations.
        /// </summary>
        /// <param name="map">A reference to the map.</param>
        /// <param name="locations">The locations to check if players can see.</param>
        /// <returns>A collection of connections.</returns>
        public static IEnumerable<IPlayer> FindPlayersThatCanSee(this IMap map, params Location[] locations)
        {
            var creaturesThatCanSee = map.FindCreaturesThatCanSee(locations);

            return creaturesThatCanSee.OfType<IPlayer>();
        }
    }
}
