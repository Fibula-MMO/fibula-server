﻿// -----------------------------------------------------------------
// <copyright file="CreatureStat.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Enumerations
{
    /// <summary>
    /// Enumerates the different creature stats.
    /// </summary>
    public enum CreatureStat
    {
        /// <summary>
        /// The hit points of a creature.
        /// </summary>
        HitPoints,

        /// <summary>
        /// The mana points of a creature.
        /// </summary>
        ManaPoints,

        /// <summary>
        /// The carry strength of a creature.
        /// </summary>
        CarryStrength,

        /// <summary>
        /// The base speed of a creature.
        /// </summary>
        BaseSpeed,

        /// <summary>
        /// The soul points of a creature.
        /// </summary>
        SoulPoints,

        /// <summary>
        /// The creature's attack points.
        /// </summary>
        AttackPoints,

        /// <summary>
        /// The creature's defense power.
        /// </summary>
        DefensePoints,
    }
}
