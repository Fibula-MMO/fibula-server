// -----------------------------------------------------------------
// <copyright file="TileFactory.cs" company="2Dudes">
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
    using Fibula.Definitions.Data.Structures;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Delegates;

    /// <summary>
    /// Class that represents a tile factory.
    /// </summary>
    public sealed class TileFactory : ITileFactory
    {
        /// <summary>
        /// Event called when a tile is created.
        /// </summary>
        public event TileFactoryTileCreatedHandler TileCreated;

        /// <summary>
        /// Creates a new <see cref="ITile"/>.
        /// </summary>
        /// <param name="atLocation">The location at which to create the tile.</param>
        /// <param name="groundItem">Optional. The starting, single ground item to set on the tile.</param>
        /// <returns>A new instance of a <see cref="ITile"/>.</returns>
        public ITile CreateTile(Location atLocation, IItem groundItem = null)
        {
            var newTile = new Tile(atLocation, groundItem);

            this.TileCreated?.Invoke(newTile);

            return newTile;
        }
    }
}
