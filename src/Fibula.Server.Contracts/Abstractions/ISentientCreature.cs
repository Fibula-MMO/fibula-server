// -----------------------------------------------------------------
// <copyright file="ISentientCreature.cs" company="2Dudes">
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
    using System.Collections.Generic;
    using Fibula.Server.Contracts.Delegates;

    /// <summary>
    /// Interface for any <see cref="ICreature"/> in the game that can perceive others.
    /// </summary>
    public interface ISentientCreature : ICreature
    {
        /// <summary>
        /// Event called when this creature senses another.
        /// </summary>
        event SentientCreatureCreaturePerceivedHandler CreatureSensed;

        /// <summary>
        /// Event called when this creature sees another.
        /// </summary>
        event SentientCreatureCreatureSeenHandler CreatureSeen;

        /// <summary>
        /// Event called when this creature loses track of a sensed creature.
        /// </summary>
        event SentientCreatureCreatureLostHandler CreatureLost;

        /// <summary>
        /// Gets the creatures who are sensed by this creature.
        /// </summary>
        IEnumerable<ICreature> PerceivedCreatures { get; }

        /// <summary>
        /// Flags a creature as being perceived.
        /// </summary>
        /// <param name="creature">The creature that is perceived.</param>
        void PerceiveCreature(ICreature creature);

        /// <summary>
        /// Flags a creature as no longer perceived.
        /// </summary>
        /// <param name="creature">The creature that was lost.</param>
        void LostCreature(ICreature creature);
    }
}
