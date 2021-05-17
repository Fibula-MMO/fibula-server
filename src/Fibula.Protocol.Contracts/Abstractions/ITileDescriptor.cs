// -----------------------------------------------------------------
// <copyright file="ITileDescriptor.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Protocol.Contracts.Abstractions
{
    using System.Collections.Generic;
    using Fibula.Protocol.Contracts;
    using Fibula.ServerV2.Contracts.Abstractions;

    /// <summary>
    /// Interface for tile descriptors, which are per protocol.
    /// </summary>
    public interface ITileDescriptor
    {
        /// <summary>
        /// Gets the description segments of a tile as seen by the given <paramref name="player"/>.
        /// </summary>
        /// <param name="player">The player for which the tile is being described.</param>
        /// <param name="tile">The tile being described.</param>
        /// <returns>A collection of description segments from the tile.</returns>
        IEnumerable<BytesSegment> DescribeTileForPlayer(IPlayer player, ITile tile);
    }
}
