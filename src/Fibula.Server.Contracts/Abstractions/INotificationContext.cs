// -----------------------------------------------------------------
// <copyright file="INotificationContext.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Abstractions
{
    using System.Threading.Tasks.Dataflow;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Interface for a notification context.
    /// </summary>
    public interface INotificationContext
    {
        /// <summary>
        /// Gets a reference to the logger in use.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Gets a reference to the map descriptor in use.
        /// </summary>
        IMapDescriptor MapDescriptor { get; }

        /// <summary>
        /// Gets the reference to the creature finder in use.
        /// </summary>
        ICreatureFinder CreatureFinder { get; }

        /// <summary>
        /// Gets the buffer to which to post the notifications to.
        /// </summary>
        IDataflowBlock Buffer { get; }
    }
}
