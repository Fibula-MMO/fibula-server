// -----------------------------------------------------------------
// <copyright file="TcpServerOptions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.TcpServer
{
    using System.ComponentModel.DataAnnotations;
    using Fibula.Common.Contracts.Models;
    using Fibula.TcpServer.Contracts.Abstractions;

    /// <summary>
    /// Class that represents options for a <see cref="TcpServer"/>.
    /// </summary>
    public sealed class TcpServerOptions : ITcpServerOptions
    {
        /// <summary>
        /// Gets or sets the id of the game world that this server will interface for.
        /// </summary>
        [Required(ErrorMessage = "A game world Id must be speficied.")]
        public string WorldId { get; set; }

        /// <summary>
        /// Gets or sets the supported client version information.
        /// </summary>
        [Required(ErrorMessage = "A supported client version must be speficied.")]
        public VersionInformation SupportedClientVersion { get; set; }

        /// <summary>
        /// Gets or sets the website url.
        /// </summary>
        public string WebsiteUrl { get; set; }
    }
}
