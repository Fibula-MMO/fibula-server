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

namespace Fibula.Server.Contracts.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Constants;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Helper class that provides extensions for the <see cref="IMap"/> implementations.
    /// </summary>
    public static class MapExtensions
    {
        /// <summary>
        /// Checks if a throw between two map locations is valid.
        /// </summary>
        /// <param name="map">A reference to the map.</param>
        /// <param name="fromLocation">The first location.</param>
        /// <param name="toLocation">The second location.</param>
        /// <param name="checkLineOfSight">Optional. A value indicating whether to consider line of sight.</param>
        /// <returns>True if the throw is valid, false otherwise.</returns>
        public static bool CanThrowBetweenLocations(this IMap map, Location fromLocation, Location toLocation, bool checkLineOfSight = true)
        {
            map.ThrowIfNull(nameof(map));

            if (fromLocation == toLocation)
            {
                return true;
            }

            if (fromLocation.Type != LocationType.Map || toLocation.Type != LocationType.Map)
            {
                return false;
            }

            // Cannot throw across the surface boundary (floor 7).
            if (Math.Max(fromLocation.Z, toLocation.Z) > MapConstants.GroundSurfaceZ &&
                Math.Min(fromLocation.Z, toLocation.Z) <= MapConstants.GroundSurfaceZ)
            {
                return false;
            }

            var deltaX = Math.Abs(fromLocation.X - toLocation.X);
            var deltaY = Math.Abs(fromLocation.Y - toLocation.Y);
            var deltaZ = Math.Abs(fromLocation.Z - toLocation.Z);

            // distance checks
            if (deltaX - deltaZ >= MapConstants.DefaultWindowSizeX / 2 || deltaY - deltaZ >= MapConstants.DefaultWindowSizeY / 2)
            {
                return false;
            }

            return !checkLineOfSight || map.AreInLineOfSight(fromLocation, toLocation) || map.AreInLineOfSight(toLocation, fromLocation);
        }

        /// <summary>
        /// Checks if two map locations are line of sight.
        /// </summary>
        /// <param name="map">A reference to the map.</param>
        /// <param name="firstLocation">The first location.</param>
        /// <param name="secondLocation">The second location.</param>
        /// <returns>True if the second location is considered within the line of sight of the first location, false otherwise.</returns>
        public static bool AreInLineOfSight(this IMap map, Location firstLocation, Location secondLocation)
        {
            map.ThrowIfNull(nameof(map));

            if (firstLocation == secondLocation)
            {
                return true;
            }

            if (firstLocation.Type != LocationType.Map || secondLocation.Type != LocationType.Map)
            {
                return false;
            }

            // Normalize so that the check always happens from 'high to low' floors.
            var origin = firstLocation.Z > secondLocation.Z ? secondLocation : firstLocation;
            var target = firstLocation.Z > secondLocation.Z ? firstLocation : secondLocation;

            // Define positive or negative steps, depending on where the target location is wrt the origin location.
            var stepX = (sbyte)(origin.X < target.X ? 1 : origin.X == target.X ? 0 : -1);
            var stepY = (sbyte)(origin.Y < target.Y ? 1 : origin.Y == target.Y ? 0 : -1);

            var a = target.Y - origin.Y;
            var b = origin.X - target.X;
            var c = -((a * target.X) + (b * target.Y));

            while ((origin - target).MaxValueIn2D != 0)
            {
                var moveHorizontal = Math.Abs((a * (origin.X + stepX)) + (b * origin.Y) + c);
                var moveVertical = Math.Abs((a * origin.X) + (b * (origin.Y + stepY)) + c);
                var moveCross = Math.Abs((a * (origin.X + stepX)) + (b * (origin.Y + stepY)) + c);

                if (origin.Y != target.Y && (origin.X == target.X || moveHorizontal > moveVertical || moveHorizontal > moveCross))
                {
                    origin.Y += stepY;
                }

                if (origin.X != target.X && (origin.Y == target.Y || moveVertical > moveHorizontal || moveVertical > moveCross))
                {
                    origin.X += stepX;
                }

                if (map.HasTileAt(origin, out ITile tile) && tile.BlocksThrow)
                {
                    return false;
                }
            }

            while (origin.Z != target.Z)
            {
                // now we need to perform a jump between floors to see if everything is clear (literally)
                if (map.HasTileAt(origin, out ITile tile) && tile.Ground != null)
                {
                    return false;
                }

                origin.Z++;
            }

            return true;
        }

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

                        if (!map.HasTileAt(loc, out ITile tile, loadAsNeeded: false))
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

                        if (!map.HasTileAt(loc, out ITile tile, loadAsNeeded: false))
                        {
                            continue;
                        }

                        foreach (var creature in tile.Creatures.ToList())
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
