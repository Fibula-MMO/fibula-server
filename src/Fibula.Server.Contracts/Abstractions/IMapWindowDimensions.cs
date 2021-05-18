// -----------------------------------------------------------------
// <copyright file="IMapWindowDimensions.cs" company="2Dudes">
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
    /// <summary>
    /// Interface for the parameters of a map loading window.
    /// </summary>
    public interface IMapWindowDimensions
    {
        /// <summary>
        /// Gets the starting value for X.
        /// </summary>
        int FromX { get; }

        /// <summary>
        /// Gets the end value for X.
        /// </summary>
        int ToX { get; }

        /// <summary>
        /// Gets the starting value for Y.
        /// </summary>
        int FromY { get; }

        /// <summary>
        /// Gets the end value for Y.
        /// </summary>
        int ToY { get; }

        /// <summary>
        /// Gets the starting value for Z.
        /// </summary>
        sbyte FromZ { get; }

        /// <summary>
        /// Gets the end value for Z.
        /// </summary>
        sbyte ToZ { get; }
    }
}
