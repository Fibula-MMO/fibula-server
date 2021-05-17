// -----------------------------------------------------------------
// <copyright file="MapConstants.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Contracts.Constants
{
    using Fibula.ServerV2.Contracts.Abstractions;

    /// <summary>
    /// Static class that contains constants regarding the map.
    /// </summary>
    public static class MapConstants
    {
        /// <summary>
        /// The maximum number of <see cref="IThing"/>s to describe per tile.
        /// </summary>
        public const int MaximumNumberOfThingsToDescribePerTile = 9;

        /// <summary>
        /// The default window size in the X coordinate.
        /// </summary>
        public const byte DefaultWindowSizeX = 18;

        /// <summary>
        /// The default window size in the Y coordinate.
        /// </summary>
        public const byte DefaultWindowSizeY = 14;

        /// <summary>
        /// The Z level considered as the ground surface.
        /// </summary>
        public const byte GroundSurfaceZ = 7;
    }
}
