// -----------------------------------------------------------------
// <copyright file="CompositionRootExtensions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Extensions
{
    using System;
    using Fibula.Data.Contracts.Abstractions;
    using Fibula.Plugins.Database.InMemoryOnly;
    using Fibula.Plugins.ItemLoaders.CipObjectsFile;
    using Fibula.Plugins.MapLoaders.CipSectorFiles;
    using Fibula.Plugins.MonsterLoaders.CipMonFiles;
    using Fibula.Plugins.PathFinding.AStar.Extensions;
    using Fibula.Plugins.SpawnLoaders.CipMonstersDbFile;
    using Fibula.Providers.Azure.Extensions;
    using Fibula.Scheduling;
    using Fibula.Scheduling.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Creatures;
    using Fibula.ServerV2.Items;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Static class that adds convenient methods to add the concrete implementations contained in this library.
    /// </summary>
    public static class CompositionRootExtensions
    {
        /// <summary>
        /// Adds all concrete class implementations contained in this library to the services collection.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The configuration reference.</param>
        public static void AddGameworldServer(this IServiceCollection services, IConfiguration configuration)
        {
            configuration.ThrowIfNull(nameof(configuration));

            // Setup game components.
            services.AddSingleton<ICreatureFactory, CreatureFactory>();
            services.AddSingleton<ICreatureManager, CreatureManager>();
            services.AddSingleton<ICreatureFinder>(s => s.GetService<ICreatureManager>());

            services.AddSingleton<IContainerManager, ContainerManager>();
            services.AddSingleton<IItemFactory, ItemFactory>();

            services.AddSingleton<IMap, Map>();
            services.AddSingleton<ITileFactory, TileFactory>();

            services.AddSingleton<IScheduler, Scheduler>();

            services.AddSingleton<GameworldService>();
            services.AddSingleton<IGameworldService>(s => s.GetService<GameworldService>());

            // Since the gmae service is derived from IHostedService, it must be added using AddHostedService
            // in order for the underlying framework to run it.
            services.AddHostedService(s => s.GetService<GameworldService>());

            // Choose a type of map loader:
            // services.AddGrassOnlyDummyMapLoader(hostingContext.Configuration);
            // services.AddOtbmMapLoader(hostingContext.Configuration);
            services.AddSectorFilesMapLoader(configuration);

            // Chose a type of item types (catalog) loader:
            services.AddObjectsFileItemTypeLoader(configuration);

            // Chose a type of monster types (catalog) loader:
            services.AddMonFilesMonsterTypeLoader(configuration);

            // Chose a type of monster spawns loader:
            services.AddMonsterDbFileMonsterSpawnLoader(configuration);

            // Chose a type of Database context:
            // services.AddCosmosDBDatabaseContext(configuration);
            // services.AddSqlServerDatabaseContext(configuration);
            services.AddInMemoryDatabaseContext(configuration);

            // IFibulaDbContext itself is added by the Add<DatabaseProvider>() call above.
            // We add Func<IFibulaDbContext> to let callers retrieve a transient instance of this from the Application context,
            // rather than save an actual copy of the DB context in the app context.
            // TODO: simplify this by adding as transient directly?
            services.AddSingleton<Func<IFibulaDbContext>>(s => s.GetService<IFibulaDbContext>);

            services.AddAStarPathFinder(configuration);

            // Azure providers for Azure VM hosting and storing secrets in KeyVault.
            services.AddAzureProviders(configuration);
        }
    }
}
