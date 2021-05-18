// -----------------------------------------------------------------
// <copyright file="ICreature.cs" company="2Dudes">
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
    using System;
    using System.Collections.Generic;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Delegates;
    using Fibula.Server.Contracts.Enumerations;

    /// <summary>
    /// Interface for all creatures in the game.
    /// </summary>
    public interface ICreature : IThing, IEquatable<ICreature>, IItemsContainer
    {
        /// <summary>
        /// Event triggered when this creature's stat has changed.
        /// </summary>
        event CreatureStatChangedHandler StatChanged;

        /// <summary>
        /// Gets or sets the parent container of this creature.
        /// </summary>
        ICreaturesContainer ParentContainer { get; set; }

        /// <summary>
        /// Gets the creature's in-game id.
        /// </summary>
        uint Id { get; }

        /// <summary>
        /// Gets the creature's blood type.
        /// </summary>
        BloodType BloodType { get; }

        /// <summary>
        /// Gets the article in the name of the creature.
        /// </summary>
        string Article { get; }

        /// <summary>
        /// Gets the name of the creature.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the creature's corpse type id.
        /// </summary>
        ushort CorpseTypeId { get; }

        /// <summary>
        /// Gets this creature's emitted light level.
        /// </summary>
        byte EmittedLightLevel { get; }

        /// <summary>
        /// Gets this creature's emitted light color.
        /// </summary>
        byte EmittedLightColor { get; }

        /// <summary>
        /// Gets this creature's movement speed.
        /// </summary>
        ushort Speed { get; }

        /// <summary>
        /// Gets this creature's flags.
        /// </summary>
        uint Flags { get; }

        /// <summary>
        /// Gets a value indicating whether the creature is dead.
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// Gets a value indicating whether this creature can walk.
        /// </summary>
        bool CanWalk { get; }

        /// <summary>
        /// Gets the creature's outfit.
        /// </summary>
        Outfit Outfit { get; }

        /// <summary>
        /// Gets the direction that the creature is facing.
        /// </summary>
        Direction Direction { get; }

        /// <summary>
        /// Gets the creature's last move modifier.
        /// </summary>
        decimal LastMovementCostModifier { get; }

        /// <summary>
        /// Gets this creature's walk plan.
        /// </summary>
        WalkPlan WalkPlan { get; }

        /// <summary>
        /// Gets the current stats information for the creature.
        /// </summary>
        /// <remarks>
        /// The key is a <see cref="CreatureStat"/>, and the value is an <see cref="IStat"/>.
        /// </remarks>
        IDictionary<CreatureStat, IStat> Stats { get; }
    }
}
