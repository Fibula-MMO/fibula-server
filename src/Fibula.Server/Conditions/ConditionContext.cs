﻿// -----------------------------------------------------------------
// <copyright file="ConditionContext.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Conditions
{
    using Fibula.Scheduling;
    using Fibula.Scheduling.Contracts.Abstractions;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a context for conditions.
    /// </summary>
    public class ConditionContext : EventContext, IConditionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionContext"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="map">A reference to the map in use.</param>
        /// <param name="creatureFinder">A reference to the creature finder in use.</param>
        /// <param name="gameOperationsApi">A reference to the game operations api.</param>
        /// <param name="itemFactory">A reference to the item factory in use.</param>
        /// <param name="scheduler">A reference to the scheduler instance.</param>
        public ConditionContext(
            ILogger logger,
            IMap map,
            IGameOperationsApi gameOperationsApi,
            ICreatureFinder creatureFinder,
            IItemFactory itemFactory,
            IScheduler scheduler)
            : base(logger, () => scheduler.CurrentTime)
        {
            map.ThrowIfNull(nameof(map));
            gameOperationsApi.ThrowIfNull(nameof(gameOperationsApi));
            creatureFinder.ThrowIfNull(nameof(creatureFinder));
            itemFactory.ThrowIfNull(nameof(itemFactory));
            scheduler.ThrowIfNull(nameof(scheduler));

            this.Map = map;
            this.GameApi = gameOperationsApi;
            this.CreatureFinder = creatureFinder;
            this.ItemFactory = itemFactory;
            this.Scheduler = scheduler;
        }

        /// <summary>
        /// Gets the reference to the map.
        /// </summary>
        public IMap Map { get; }

        /// <summary>
        /// Gets a reference to the game's api.
        /// </summary>
        public IGameOperationsApi GameApi { get; }

        /// <summary>
        /// Gets the reference to the creature finder in use.
        /// </summary>
        public ICreatureFinder CreatureFinder { get; }

        /// <summary>
        /// Gets a reference to the item factory in use.
        /// </summary>
        public IItemFactory ItemFactory { get; }

        /// <summary>
        /// Gets a reference to the scheduler in use.
        /// </summary>
        public IScheduler Scheduler { get; }
    }
}
