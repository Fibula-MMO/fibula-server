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

namespace Fibula.Server.GameService
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Class that represents the startup point in the application.
    /// </summary>
    public class Startup
    {
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

            services.AddSingleton<IGame, GameMock>();

            services.AddSingleton<IHostedService>(s => s.GetRequiredService<IGame>());
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
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
