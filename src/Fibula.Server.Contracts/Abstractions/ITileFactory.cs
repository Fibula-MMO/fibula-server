// -----------------------------------------------------------------
// <copyright file="ITileFactory.cs" company="2Dudes">
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
    using Fibula.Definitions.Data.Structures;

    /// <summary>
    /// Interface for a tile factory.
    /// </summary>
    public interface ITileFactory
    {
        /// <summary>
        /// Creates a new <see cref="ITile"/>.
        /// </summary>
        /// <param name="atLocation">The location at which to create the tile.</param>
        /// <param name="groundItem">Optional. The starting, single ground item to set on the tile.</param>
        /// <returns>A new instance of a <see cref="ITile"/>.</returns>
        ITile CreateTile(Location atLocation, IItem groundItem = null);
    }
}
