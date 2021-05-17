// -----------------------------------------------------------------
// <copyright file="NotificationSentHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.TcpServer.Contracts.Delegates
{
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Delegate for an event fired by an <see cref="INotification"/> when it's been sent.
    /// </summary>
    /// <param name="toPlayer">The client to which the notification was sent.</param>
    public delegate void NotificationSentHandler(IPlayer toPlayer);
}
