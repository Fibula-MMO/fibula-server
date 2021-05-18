// -----------------------------------------------------------------
// <copyright file="SentientCreatureCreatureSeenHandler.cs" company="2Dudes">
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
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Delegate meant for when a creature sees a new creature.
    /// </summary>
    /// <param name="creature">The creature that sees the other.</param>
    /// <param name="creatureSeen">The creature that was seen.</param>
    public delegate void SentientCreatureCreatureSeenHandler(ISentientCreature creature, ICreature creatureSeen);
}
