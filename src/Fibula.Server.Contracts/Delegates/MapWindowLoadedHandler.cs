// -----------------------------------------------------------------
// <copyright file="MapWindowLoadedHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Delegates
{
    /// <summary>
    /// Delegate to handle a window in the map being loaded.
    /// </summary>
    /// <param name="fromX">The start X coordinate for the load window.</param>
    /// <param name="toX">The end X coordinate for the load window.</param>
    /// <param name="fromY">The start Y coordinate for the load window.</param>
    /// <param name="toY">The end Y coordinate for the load window.</param>
    /// <param name="fromZ">The start Z coordinate for the load window.</param>
    /// <param name="toZ">The end Z coordinate for the load window.</param>
    public delegate void MapWindowLoadedHandler(int fromX, int toX, int fromY, int toY, sbyte fromZ, sbyte toZ);
}
