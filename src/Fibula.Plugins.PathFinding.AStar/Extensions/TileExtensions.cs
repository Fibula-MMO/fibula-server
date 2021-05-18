// -----------------------------------------------------------------
// <copyright file="TileExtensions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Plugins.PathFinding.AStar.Extensions
{
    using System.Linq;
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Class that contains extension methods for an <see cref="ITile"/>, regarding pathfinding.
    /// </summary>
    public static class TileExtensions
    {
        /// <summary>
        /// Determines if a tile is considered to be blocking the path.
        /// </summary>
        /// <param name="tile">The tile to evaluate.</param>
        /// <returns>True if the tile is considered path blocking, false otherwise.</returns>
        public static bool IsPathBlocking(this ITile tile)
        {
            var blocking = tile.BlocksPass;

            if (blocking)
            {
                return true;
            }

            var (fixedItems, items) = tile.GetItemsToDescribeByPriority(-1);

            blocking |= tile.Creatures.Any() || fixedItems.Any(i => i.IsPathBlocking()) || items.Any(i => i.IsPathBlocking());

            return blocking;
        }
    }
}
