// -----------------------------------------------------------------
// <copyright file="IConditionContext.cs" company="2Dudes">
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
    using Fibula.Scheduling.Contracts.Abstractions;

    /// <summary>
    /// Interface for a condition context.
    /// </summary>
    public interface IConditionContext : IEventContext
    {
        /// <summary>
        /// Gets a reference to the map descriptor in use.
        /// </summary>
        IMapDescriptor MapDescriptor { get; }

        /// <summary>
        /// Gets the reference to the map.
        /// </summary>
        IMap Map { get; }

        /// <summary>
        /// Gets a reference to the game's api.
        /// </summary>
        IGameOperationsApi GameApi { get; }

        /// <summary>
        /// Gets the reference to the creature finder in use.
        /// </summary>
        ICreatureFinder CreatureFinder { get; }

        /// <summary>
        /// Gets a reference to the item factory in use.
        /// </summary>
        IItemFactory ItemFactory { get; }

        /// <summary>
        /// Gets a reference to the scheduler in use.
        /// </summary>
        IScheduler Scheduler { get; }
    }
}
