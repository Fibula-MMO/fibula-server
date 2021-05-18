// -----------------------------------------------------------------
// <copyright file="ThingLocationChangedHandler.cs" company="2Dudes">
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
    using Fibula.Definitions.Data.Structures;
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Delegate to handle an <see cref="IThing"/>'s location changing.
    /// </summary>
    /// <param name="thing">A reference to the thing which's location changed.</param>
    /// <param name="previousLocation">The thing's previous location.</param>
    public delegate void ThingLocationChangedHandler(IThing thing, Location previousLocation);
}
