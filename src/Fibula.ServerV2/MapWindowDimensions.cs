// -----------------------------------------------------------------
// <copyright file="MapWindowDimensions.cs" company="2Dudes">
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
    using Fibula.ServerV2.Contracts.Abstractions;

    /// <summary>
    /// Class that represents the dimensions for a map window.
    /// </summary>
    public sealed class MapWindowDimensions : IMapWindowDimensions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapWindowDimensions"/> class.
        /// </summary>
        /// <param name="fromX">The X dimension from where the window starts.</param>
        /// <param name="fromY">The Y dimension from where the window starts.</param>
        /// <param name="fromZ">The Z dimension from where the window starts.</param>
        /// <param name="toX">Optional. The X dimension at where the window ends. Defaults to <paramref name="fromX"/>, creating a dimension of 1 in X.</param>
        /// <param name="toY">Optional. The Y dimension at where the window ends. Defaults to <paramref name="fromY"/>, creating a dimension of 1 in Y.</param>
        /// <param name="toZ">Optional. The Z dimension at where the window ends. Defaults to <paramref name="fromZ"/>, creating a dimension of 1 in Z.</param>
        public MapWindowDimensions(int fromX, int fromY, sbyte fromZ, int? toX = null, int? toY = null, sbyte? toZ = null)
        {
            this.FromX = fromX;
            this.FromY = fromY;
            this.FromZ = fromZ;

            this.ToX = toX ?? fromX;
            this.ToY = toY ?? fromY;
            this.ToZ = toZ ?? fromZ;
        }

        /// <summary>
        /// Gets the starting value for X.
        /// </summary>
        public int FromX { get; }

        /// <summary>
        /// Gets the end value for X.
        /// </summary>
        public int ToX { get; }

        /// <summary>
        /// Gets the starting value for Y.
        /// </summary>
        public int FromY { get; }

        /// <summary>
        /// Gets the end value for Y.
        /// </summary>
        public int ToY { get; }

        /// <summary>
        /// Gets the starting value for Z.
        /// </summary>
        public sbyte FromZ { get; }

        /// <summary>
        /// Gets the end value for Z.
        /// </summary>
        public sbyte ToZ { get; }
    }
}
