// -----------------------------------------------------------------
// <copyright file="IOperationContext.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Contracts.Abstractions
{
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Scheduling.Contracts.Abstractions;

    /// <summary>
    /// Interface for an operation context.
    /// </summary>
    public interface IOperationContext : IEventContext
    {
        /// <summary>
        /// Gets the reference to the map.
        /// </summary>
        IMap Map { get; }

        /// <summary>
        /// Gets the reference to the application context.
        /// </summary>
        IApplicationContext ApplicationContext { get; }

        /// <summary>
        /// Gets the reference to the creature manager in use.
        /// </summary>
        ICreatureManager CreatureManager { get; }

        /// <summary>
        /// Gets a reference to the item factory in use.
        /// </summary>
        IItemFactory ItemFactory { get; }

        /// <summary>
        /// Gets a reference to the creature factory in use.
        /// </summary>
        ICreatureFactory CreatureFactory { get; }

        /// <summary>
        /// Gets a reference to the game's api.
        /// </summary>
        IGameOperationsApi GameApi { get; }

        /// <summary>
        /// Gets a reference to the pathfinder algorithm in use.
        /// </summary>
        IPathFinder PathFinder { get; }

        /// <summary>
        /// Gets the predefined item set.
        /// </summary>
        IPredefinedItemSet PredefinedItemSet { get; }

        /// <summary>
        /// Gets a reference to the scheduler in use.
        /// </summary>
        IScheduler Scheduler { get; }
    }
}
