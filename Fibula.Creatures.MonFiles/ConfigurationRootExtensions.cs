﻿// -----------------------------------------------------------------
// <copyright file="ConfigurationRootExtensions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Creatures.MonFiles
{
    using Fibula.Common.Utilities;
    using Fibula.Creatures.Contracts.Abstractions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Static class that adds convenient methods to add the concrete implementations contained in this library.
    /// </summary>
    public static class ConfigurationRootExtensions
    {
        /// <summary>
        /// Adds all implementations related to *.mon monster type files contained in this library to the services collection.
        /// Additionally, registers the options related to the concrete implementations added, such as:
        ///     <see cref="MonFilesMonsterTypeLoaderOptions"/>.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The configuration reference.</param>
        public static void AddMonFilesMonsterTypeLoader(this IServiceCollection services, IConfiguration configuration)
        {
            configuration.ThrowIfNull(nameof(configuration));

            // configure options
            services.Configure<MonFilesMonsterTypeLoaderOptions>(configuration.GetSection(nameof(MonFilesMonsterTypeLoaderOptions)));

            services.AddSingleton<IMonsterTypeLoader, MonFilesMonsterTypeLoader>();
        }
    }
}