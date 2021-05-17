// -----------------------------------------------------------------
// <copyright file="NotificationContext.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Notifications
{
    using System.Threading.Tasks.Dataflow;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a context for notifications.
    /// </summary>
    public class NotificationContext : INotificationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationContext"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="mapDescriptor">A reference to the map descriptor in use.</param>
        /// <param name="creatureFinder">A reference to the creature finder in use.</param>
        /// <param name="buffer">A buffer that accepts the prepared notifications.</param>
        public NotificationContext(ILogger logger, IMapDescriptor mapDescriptor, ICreatureFinder creatureFinder, BufferBlock<GameNotification> buffer)
        {
            logger.ThrowIfNull(nameof(logger));
            mapDescriptor.ThrowIfNull(nameof(mapDescriptor));
            creatureFinder.ThrowIfNull(nameof(creatureFinder));
            buffer.ThrowIfNull(nameof(buffer));

            this.Logger = logger;
            this.MapDescriptor = mapDescriptor;
            this.CreatureFinder = creatureFinder;
            this.Buffer = buffer;
        }

        /// <summary>
        /// Gets a reference to the logger in use.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets the map descriptor in use.
        /// </summary>
        public IMapDescriptor MapDescriptor { get; }

        /// <summary>
        /// Gets the creature finder in use.
        /// </summary>
        public ICreatureFinder CreatureFinder { get; }

        /// <summary>
        /// Gets the data buffer at which the notification should be posted.
        /// </summary>
        public IDataflowBlock Buffer { get; }
    }
}
