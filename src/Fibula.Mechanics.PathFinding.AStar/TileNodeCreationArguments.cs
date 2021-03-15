// -----------------------------------------------------------------
// <copyright file="TileNodeCreationArguments.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.PathFinding.AStar
{
    using Fibula.Definitions.Data.Structures;
    using Fibula.Utilities.Pathfinding.Abstractions;

    /// <summary>
    /// Class that represents arguments for <see cref="TileNode"/> creation.
    /// </summary>
    internal class TileNodeCreationArguments : INodeCreationArguments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TileNodeCreationArguments"/> class.
        /// </summary>
        /// <param name="location">The location of the tile.</param>
        public TileNodeCreationArguments(Location location)
        {
            this.Location = location;
        }

        /// <summary>
        /// Gets the location of the tile.
        /// </summary>
        public Location Location { get; }
    }
}
