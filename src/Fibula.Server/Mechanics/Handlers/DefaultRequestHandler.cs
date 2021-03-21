// -----------------------------------------------------------------
// <copyright file="DefaultRequestHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Mechanics.Handlers
{
    using System.Collections.Generic;
    using System.Text;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Special kind of handler that is used as a fall back when no other handler is picked.
    /// </summary>
    public sealed class DefaultRequestHandler : BaseRequestHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestHandler"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public DefaultRequestHandler(ILogger<DefaultRequestHandler> logger)
            : base(logger)
        {
        }

        /// <summary>
        /// Handles the contents of a network message.
        /// </summary>
        /// <param name="incomingPacket">The packet to handle.</param>
        /// <param name="client">A reference to the client from where this request originated from, for context.</param>
        /// <returns>A collection of <see cref="IOutboundPacket"/>s that compose that synchronous response, if any.</returns>
        public override IEnumerable<IOutboundPacket> HandleRequest(IIncomingPacket incomingPacket, IClient client)
        {
            incomingPacket.ThrowIfNull(nameof(incomingPacket));
            client.ThrowIfNull(nameof(client));

            if (!(incomingPacket is IBytesInfo debugInfo))
            {
                this.Logger.LogError($"Expected packet info of type {nameof(IBytesInfo)} but got {incomingPacket.GetType().Name}.");

                return null;
            }

            var sb = new StringBuilder();

            foreach (var b in debugInfo.Bytes)
            {
                sb.AppendFormat("{0:x2} ", b);
            }

            this.Logger.LogInformation($"Default handler drained packet with content:\n\n{sb}");

            return null;
        }
    }
}
