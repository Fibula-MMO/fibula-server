// -----------------------------------------------------------------
// <copyright file="Program.cs" company="2Dudes">
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
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Fibula.Common;
    using Fibula.Common.Contracts;
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Server.Extensions;
    using Fibula.TcpServer.Contracts.Abstractions;
    using Fibula.TcpServer.Extensions;
    using Fibula.Utilities.Validation;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Serilog;

    /// <summary>
    /// Class that represents a standalone Fibula server instance.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The prefix used to identify enviroment variables loaded on configuration.
        /// </summary>
        private const string EnvironmentVariablesPrefix = "FIBULA_";

        /// <summary>
        /// The cancellation token source for the entire application.
        /// </summary>
        private static readonly CancellationTokenSource MasterCancellationTokenSource = new ();

        /// <summary>
        /// The main entry point for the program.
        /// </summary>
        /// <param name="args">The arguments for this program.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("logsettings.json")
                .Build();

            var serverHost = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("hostsettings.json", optional: true, reloadOnChange: true);
                    configHost.AddEnvironmentVariables(prefix: EnvironmentVariablesPrefix);
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(Directory.GetCurrentDirectory());
                    configApp.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    configApp.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    configApp.AddEnvironmentVariables(prefix: EnvironmentVariablesPrefix);
                    configApp.AddCommandLine(args);
                })
                .ConfigureServices(ConfigureServices)
                .UseSerilog((context, services, loggerConfig) =>
                {
                    var telemetryConfigOptions = services.GetRequiredService<IOptions<TelemetryConfiguration>>();

                    loggerConfig.ReadFrom.Configuration(configuration)
                                .WriteTo.ApplicationInsights(telemetryConfigOptions.Value, TelemetryConverter.Traces, Serilog.Events.LogEventLevel.Information)
                                .Enrich.FromLogContext();
                })
                .Build();

            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;

            await serverHost.RunAsync(Program.MasterCancellationTokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles an unobserved task exception.
        /// </summary>
        /// <param name="sender">The sender of the exception.</param>
        /// <param name="e">The exception arguments.</param>
        private static void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
        }

        /// <summary>
        /// Composition root, where services are configured and added into the service collection, often depending on the configuration set.
        /// </summary>
        /// <param name="hostingContext">The hosting context.</param>
        /// <param name="services">The services collection.</param>
        private static void ConfigureServices(HostBuilderContext hostingContext, IServiceCollection services)
        {
            hostingContext.ThrowIfNull(nameof(hostingContext));
            services.ThrowIfNull(nameof(services));

            // Configure options here
            services.Configure<ApplicationContextOptions>(hostingContext.Configuration.GetSection(nameof(ApplicationContextOptions)));
            services.Configure<TelemetryConfiguration>(hostingContext.Configuration.GetSection(nameof(TelemetryConfiguration)));

            services.AddApplicationInsightsTelemetryWorkerService();

            services.AddSingleton<IApplicationContext, ApplicationContext>();

            // The master cancellation token source for the entire application.
            services.AddSingleton(Program.MasterCancellationTokenSource);

            services.AddGameworldServer(hostingContext.Configuration);

            services.AddTcpServer(hostingContext.Configuration);

            services.AddSingleton<ITcpServerToGameworldAdapter, TcpServerToGameworldAdapter>();
        }
    }
}
