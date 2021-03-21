// -----------------------------------------------------------------
// <copyright file="ICombatOperationsApi.cs" company="2Dudes">
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
    using Fibula.Definitions.Enumerations;

    /// <summary>
    /// Interface for the API of combat operations.
    /// </summary>
    public interface ICombatOperationsApi
    {
        /// <summary>
        /// Sets the fight, chase and safety modes of a combatant.
        /// </summary>
        /// <param name="combatant">The combatant that update modes.</param>
        /// <param name="fightMode">The fight mode to change to.</param>
        /// <param name="chaseMode">The chase mode to change to.</param>
        /// <param name="safeModeOn">A value indicating whether the attack safety lock is on.</param>
        void SetCombatantModes(ICombatant combatant, FightMode fightMode, ChaseMode chaseMode, bool safeModeOn);

        /// <summary>
        /// Sets the combat target of the attacker and it's (possibly new) target.
        /// </summary>
        /// <param name="attacker">The attacker.</param>
        /// <param name="target">The target.</param>
        void SetCombatantAttackTarget(ICombatant attacker, ICombatant target);

        /// <summary>
        /// Re-sets the follow target of the combatant and it's (possibly new) target.
        /// </summary>
        /// <param name="combatant">The attacker.</param>
        /// <param name="target">The new target.</param>
        void SetCombatantFollowTarget(ICombatant combatant, ICombatant target);
    }
}
