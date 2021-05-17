// -----------------------------------------------------------------
// <copyright file="ITile.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Contracts.Abstractions
{
    using System;
    using System.Collections.Generic;
    using Fibula.ServerV2.Contracts.Constants;

    /// <summary>
    /// Interface for all tiles.
    /// </summary>
    public interface ITile : ILocatable, IThingsContainer
    {
        /// <summary>
        /// Gets the last date and time that this tile was modified.
        /// </summary>
        DateTimeOffset LastModified { get; }

        /// <summary>
        /// Gets the single ground item that a tile may have.
        /// </summary>
        IItem Ground { get; }

        /// <summary>
        /// Gets the single liquid pool item that a tile may have.
        /// </summary>
        IItem LiquidPool { get; }

        /// <summary>
        /// Gets a value indicating whether items in this tile block throwing.
        /// </summary>
        bool BlocksThrow { get; }

        /// <summary>
        /// Gets a value indicating whether items in this tile block passing.
        /// </summary>
        bool BlocksPass { get; }

        /// <summary>
        /// Gets a value indicating whether items in this tile block laying.
        /// </summary>
        bool BlocksLay { get; }

        /// <summary>
        /// Gets the thing that is on top based on the tile's stack order.
        /// </summary>
        IThing TopThing { get; }

        /// <summary>
        /// Gets the item that is on top based on the tile's stack order.
        /// </summary>
        IItem TopItem { get; }

        /// <summary>
        /// Gets the creature that is on top based on the tile's stack order.
        /// </summary>
        ICreature TopCreature { get; }

        /// <summary>
        /// Gets the tile's creatures.
        /// </summary>
        IEnumerable<ICreature> Creatures { get; }

        /// <summary>
        /// Gets the flags from this tile.
        /// </summary>
        byte Flags { get; }

        /// <summary>
        /// Attempts to get the tile's items to describe prioritized and ordered by their stack order.
        /// </summary>
        /// <param name="maxItemsToGet">The maximum number of items to include in the result.</param>
        /// <returns>The items in the tile, split by those which are fixed and those considered normal.</returns>
        (IEnumerable<IItem> fixedItems, IEnumerable<IItem> normalItems) GetItemsToDescribeByPriority(int maxItemsToGet = MapConstants.MaximumNumberOfThingsToDescribePerTile);

        ///// <summary>
        ///// Sets a flag on this tile.
        ///// </summary>
        ///// <param name="flag">The flag to set.</param>
        // void SetFlag(TileFlag flag);
    }
}
