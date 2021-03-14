// -----------------------------------------------------------------
// <copyright file="CharacterStat.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Data.Entities.Contracts.Enumerations
{
    /// <summary>
    /// Enumerates the different character stats.
    /// </summary>
    public enum CharacterStat
    {
        /// <summary>
        /// The hit points of a character.
        /// </summary>
        HitPoints,

        /// <summary>
        /// The maximum hit points of a character.
        /// </summary>
        MaximumHitPoints,

        /// <summary>
        /// The mana points of a character.
        /// </summary>
        ManaPoints,

        /// <summary>
        /// The maximum mana points of a character.
        /// </summary>
        MaximumManaPoints,

        /// <summary>
        /// The soul points of a character.
        /// </summary>
        SoulPoints,

        /// <summary>
        /// The carry strength of a character.
        /// </summary>
        CarryStrength,

        /// <summary>
        /// The base speed of a character.
        /// </summary>
        BaseSpeed,
    }
}
