// -----------------------------------------------------------------
// <copyright file="ProfessionType.cs" company="2Dudes">
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
    /// Enumeration of the possible professions in the game.
    /// </summary>
    public enum ProfessionType : byte
    {
        /// <summary>
        /// No profession.
        /// </summary>
        None,

        /// <summary>
        /// A melee figther.
        /// </summary>
        Knight,

        /// <summary>
        /// A distance fighter.
        /// </summary>
        Paladin,

        /// <summary>
        /// A combat magic caster.
        /// </summary>
        Sorcerer,

        /// <summary>
        /// A nature magic caster.
        /// </summary>
        Druid,
    }
}
