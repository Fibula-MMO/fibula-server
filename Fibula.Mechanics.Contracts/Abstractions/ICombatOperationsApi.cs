﻿// -----------------------------------------------------------------
// <copyright file="ICombatOperationsApi.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Mechanics.Contracts.Abstractions
{
    using Fibula.Common.Contracts.Enumerations;

    /// <summary>
    /// Interface for the API of combat operations.
    /// </summary>
    public interface ICombatOperationsApi
    {
        /// <summary>
        /// Handles a health change event from a combatant.
        /// </summary>
        /// <param name="combatant">The combatant who's health changed.</param>
        /// <param name="oldHealthValue">The old value of the combatant's health.</param>
        void CombatantHealthChanged(ICombatant combatant, ushort oldHealthValue);

        /// <summary>
        /// Handles a death from a combatant.
        /// </summary>
        /// <param name="combatant">The combatant that died.</param>
        void CombatantDeath(ICombatant combatant);

        /// <summary>
        /// Handles an attack target change from a combatant.
        /// </summary>
        /// <param name="combatant">The combatant that died.</param>
        /// <param name="oldTarget">The previous attack target, which can be null.</param>
        void CombatantAttackTargetChanged(ICombatant combatant, ICombatant oldTarget);

        /// <summary>
        /// Handles a chase target change from a combatant.
        /// </summary>
        /// <param name="combatant">The combatant that died.</param>
        /// <param name="oldTarget">The previous chased target, which can be null.</param>
        void CombatantChaseTargetChanged(ICombatant combatant, ICombatant oldTarget);

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
    }
}
