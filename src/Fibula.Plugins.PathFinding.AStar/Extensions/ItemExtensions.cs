// -----------------------------------------------------------------
// <copyright file="ItemExtensions.cs" company="2Dudes">
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
    using Fibula.Definitions.Flags;
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Class that contains extension methods for an <see cref="IItem"/>, regarding pathfinding.
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        /// Determines if this item is blocks pathfinding.
        /// </summary>
        /// <param name="item">The item to evaluate.</param>
        /// <returns>True if the tile is considered path blocking, false otherwise.</returns>
        public static bool IsPathBlocking(this IItem item)
        {
            var blocking = item.BlocksPass;

            if (blocking)
            {
                return true;
            }

            blocking |= item.Type.HasItemFlag(ItemFlag.ShouldBeAvoided); // && (Convert.ToByte(item.Attributes[ItemAttribute.DamageTypesToAvoid]) ^ avoidTypes) > 0;

            return blocking;
        }
    }
}
