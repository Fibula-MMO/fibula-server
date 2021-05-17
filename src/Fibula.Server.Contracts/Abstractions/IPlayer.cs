// -----------------------------------------------------------------
// <copyright file="IPlayer.cs" company="2Dudes">
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
    /// Interface for character players in the game.
    /// </summary>
    public interface IPlayer : ICreatureWithSkills
    {
        /// <summary>
        /// Gets the player's character id.
        /// </summary>
        string CharacterId { get; }

        /// <summary>
        /// Gets the player's permissions level.
        /// </summary>
        byte PermissionsLevel { get; }

        /// <summary>
        /// Gets the player's soul points.
        /// </summary>
        // TODO: nobody likes soulpoints... figure out what to do with them.
        byte SoulPoints { get; }

        /// <summary>
        /// Gets the player's profession.
        /// </summary>
        ProfessionType Profession { get; }
    }
}
