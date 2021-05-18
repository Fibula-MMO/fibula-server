// -----------------------------------------------------------------
// <copyright file="OperationContext.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Operations
{
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Scheduling;
    using Fibula.Scheduling.Contracts.Abstractions;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a context for operations.
    /// </summary>
    public class OperationContext : EventContext, IOperationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationContext"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="applicationContext">A reference to the application context.</param>
        /// <param name="map">A reference to the map in use.</param>
        /// <param name="creatureManager">A reference to the creature finder in use.</param>
        /// <param name="itemFactory">A reference to the item factory in use.</param>
        /// <param name="creatureFactory">A reference to the creature factory in use.</param>
        /// <param name="containerManager">A reference to the container manager in use.</param>
        /// <param name="gameOperationsApi">A reference to the game operations api.</param>
        /// <param name="pathFinderAlgo">A reference to the path finding algorithm in use.</param>
        /// <param name="predefinedItemSet">A reference to the predefined item set declared.</param>
        /// <param name="scheduler">A reference to the scheduler instance.</param>
        public OperationContext(
            ILogger logger,
            IApplicationContext applicationContext,
            IMap map,
            ICreatureManager creatureManager,
            IItemFactory itemFactory,
            ICreatureFactory creatureFactory,
            IContainerManager containerManager,
            IGameOperationsApi gameOperationsApi,
            IPathFinder pathFinderAlgo,
            IPredefinedItemSet predefinedItemSet,
            IScheduler scheduler)
            : base(logger, () => scheduler.CurrentTime)
        {
            applicationContext.ThrowIfNull(nameof(applicationContext));
            map.ThrowIfNull(nameof(map));
            creatureManager.ThrowIfNull(nameof(creatureManager));
            itemFactory.ThrowIfNull(nameof(itemFactory));
            creatureFactory.ThrowIfNull(nameof(creatureFactory));
            containerManager.ThrowIfNull(nameof(containerManager));
            gameOperationsApi.ThrowIfNull(nameof(gameOperationsApi));
            pathFinderAlgo.ThrowIfNull(nameof(pathFinderAlgo));
            predefinedItemSet.ThrowIfNull(nameof(predefinedItemSet));
            scheduler.ThrowIfNull(nameof(scheduler));

            this.ApplicationContext = applicationContext;
            this.Map = map;
            this.CreatureManager = creatureManager;
            this.ItemFactory = itemFactory;
            this.CreatureFactory = creatureFactory;
            this.ContainerManager = containerManager;
            this.GameApi = gameOperationsApi;
            this.PathFinder = pathFinderAlgo;
            this.PredefinedItemSet = predefinedItemSet;
            this.Scheduler = scheduler;
        }

        /// <summary>
        /// Gets the reference to the application context.
        /// </summary>
        public IApplicationContext ApplicationContext { get; }

        /// <summary>
        /// Gets the reference to the map.
        /// </summary>
        public IMap Map { get; }

        /// <summary>
        /// Gets the reference to the creature manager in use.
        /// </summary>
        public ICreatureManager CreatureManager { get; }

        /// <summary>
        /// Gets a reference to the item factory in use.
        /// </summary>
        public IItemFactory ItemFactory { get; }

        /// <summary>
        /// Gets a reference to the creature factory in use.
        /// </summary>
        public ICreatureFactory CreatureFactory { get; }

        /// <summary>
        /// Gets a reference to the container manager in use.
        /// </summary>
        public IContainerManager ContainerManager { get; }

        /// <summary>
        /// Gets a reference to the game's api.
        /// </summary>
        public IGameOperationsApi GameApi { get; }

        /// <summary>
        /// Gets a reference to the pathfinder algorithm in use.
        /// </summary>
        public IPathFinder PathFinder { get; }

        /// <summary>
        /// Gets the predefined item set.
        /// </summary>
        public IPredefinedItemSet PredefinedItemSet { get; }

        /// <summary>
        /// Gets a reference to the scheduler in use.
        /// </summary>
        public IScheduler Scheduler { get; }
    }
}
