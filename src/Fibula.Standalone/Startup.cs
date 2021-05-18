// -----------------------------------------------------------------
// <copyright file="Startup.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Standalone
{
    using System;
    using System.Threading;
    using Fibula.Common;
    using Fibula.Common.Contracts;
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Data.Contracts.Abstractions;
    using Fibula.Plugins.Database.InMemoryOnly;
    using Fibula.Plugins.ItemLoaders.CipObjectsFile;
    using Fibula.Plugins.MapLoaders.CipSectorFiles;
    using Fibula.Plugins.MonsterLoaders.CipMonFiles;
    using Fibula.Plugins.PathFinding.AStar.Extensions;
    using Fibula.Plugins.SpawnLoaders.CipMonstersDbFile;
    using Fibula.Providers.Azure.Extensions;
    using Fibula.Server;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Creatures;
    using Fibula.Server.Extensions;
    using Fibula.Server.Items;
    using Fibula.Server.Mechanics;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Class that represents the startup point in the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The cancellation token source for the entire application.
        /// </summary>
        private static readonly CancellationTokenSource MasterCancellationTokenSource = new CancellationTokenSource();

        private readonly IWebHostEnvironment environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The service configuration in use.</param>
        /// <param name="env">A reference to the environment representation.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this.Configuration = configuration;
            this.environment = env;
        }

        /// <summary>
        /// Gets a reference to the service configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Injects and configures components into the service collection.
        /// </summary>
        /// <param name="services">The service collection to inject services into.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc((options) =>
            {
                // TODO: remove for prod
                options.EnableDetailedErrors = true;
            });

            // Add the master cancellation token source of the entire service.
            services.AddSingleton(MasterCancellationTokenSource);

            // Configure options here
            services.Configure<ApplicationContextOptions>(this.Configuration.GetSection(nameof(ApplicationContextOptions)));
            services.Configure<TelemetryConfiguration>(this.Configuration.GetSection(nameof(TelemetryConfiguration)));

            services.AddSingleton<IApplicationContext, ApplicationContext>();

            services.AddGameworldServer(this.Configuration);

            this.ConfigureMap(services);

            this.ConfigureItems(services);

            this.ConfigureCreatures(services);

            this.ConfigureDatabaseContext(services);

            this.ConfigurePathFindingAlgorithm(services);

            this.ConfigureOtherServices(services);
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// </summary>
        /// <param name="app">A reference to the application builder.</param>
        /// <param name="env">A reference to the web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GameService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Unsupported request at gRPC endpoint.");
                });
            });
        }
    }
}
