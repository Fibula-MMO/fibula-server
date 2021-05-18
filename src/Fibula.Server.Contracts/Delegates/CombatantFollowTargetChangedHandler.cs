// -----------------------------------------------------------------
// <copyright file="CombatantFollowTargetChangedHandler.cs" company="2Dudes">
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
    /// Delegate meant for when a combatant changes follow targets.
    /// </summary>
    /// <param name="combatant">The combatant that changed follow target.</param>
    /// <param name="oldTarget">The old follow target, if any.</param>
    public delegate void CombatantFollowTargetChangedHandler(ICombatant combatant, ICreature oldTarget);
}
