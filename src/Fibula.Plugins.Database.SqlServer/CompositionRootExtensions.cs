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

namespace Fibula.Plugins.Database.SqlServer
{
    using Fibula.Data.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Static class that adds convenient methods to add the concrete implementations contained in this library.
    /// </summary>
    public static class CompositionRootExtensions
    {
        /// <summary>
        /// Adds all implementations related to the SqlServer database contained in this library to the services collection.
        /// Additionally, registers the options related to the concrete implementations added, such as:
        ///     <see cref="FibulaSqlServerContextOptions"/>.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The configuration reference.</param>
        public static void AddSqlServerDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            configuration.ThrowIfNull(nameof(configuration));

            // configure options
            services.Configure<FibulaSqlServerContextOptions>(configuration.GetSection(nameof(FibulaSqlServerContextOptions)));

            services.AddTransient<IFibulaDbContext, FibulaSqlServerContext>();
        }
    }
}
