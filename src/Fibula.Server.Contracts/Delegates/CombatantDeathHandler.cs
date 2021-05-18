// -----------------------------------------------------------------
// <copyright file="CombatantDeathHandler.cs" company="2Dudes">
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
    /// Delegate meant for when a combatant dies.
    /// </summary>
    /// <param name="combatant">The combatant that died.</param>
    public delegate void CombatantDeathHandler(ICombatant combatant);
}
