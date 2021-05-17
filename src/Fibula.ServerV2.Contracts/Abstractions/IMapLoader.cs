// -----------------------------------------------------------------
// <copyright file="IMapLoader.cs" company="2Dudes">
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
    using System.Collections.Generic;

    /// <summary>
    /// Common interface for map loaders.
    /// </summary>
    public interface IMapLoader
    {
        /// <summary>
        /// Gets the percentage completed loading the map [0, 100].
        /// </summary>
        byte PercentageComplete { get; }

        /// <summary>
        /// Computes a hash string using the given window parameters.
        /// </summary>
        /// <param name="dimensions">The dimensions to compute the hash with.</param>
        /// <returns>A hash represented by a string.</returns>
        string GetLoadHash(IMapWindowDimensions dimensions);

        /// <summary>
        /// Attempts to load all tiles within a map window.
        /// </summary>
        /// <param name="window">The parameters to for the window to load.</param>
        /// <returns>A collection of <see cref="ITile"/>s loaded.</returns>
        IEnumerable<ITile> Load(IMapWindowDimensions window);
    }
}
