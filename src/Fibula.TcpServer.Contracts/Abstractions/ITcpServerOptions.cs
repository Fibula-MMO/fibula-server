// -----------------------------------------------------------------
// <copyright file="ITcpServerOptions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.TcpServer.Contracts.Abstractions
{
    using Fibula.Common.Contracts.Models;

    /// <summary>
    /// Interface for options for an <see cref="ITcpServer"/>.
    /// </summary>
    public interface ITcpServerOptions
    {
        /// <summary>
        /// Gets or sets the id of the game world that this server will interface for.
        /// </summary>
        string WorldId { get; set; }

        /// <summary>
        /// Gets or sets the supported client version information.
        /// </summary>
        VersionInformation SupportedClientVersion { get; set; }

        /// <summary>
        /// Gets or sets the website url.
        /// </summary>
        string WebsiteUrl { get; set; }
    }
}
