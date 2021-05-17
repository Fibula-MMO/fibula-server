// -----------------------------------------------------------------
// <copyright file="IWorldInformation.cs" company="2Dudes">
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
    using Fibula.Server.Contracts.Enumerations;

    /// <summary>
    /// Interface for the world information.
    /// </summary>
    public interface IWorldInformation
    {
        /// <summary>
        /// Gets the id of the game world.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the game world's light color.
        /// </summary>
        byte LightColor { get; }

        /// <summary>
        /// Gets the game world's light level.
        /// </summary>
        byte LightLevel { get; }

        /// <summary>
        /// Gets the game world's state.
        /// </summary>
        WorldState Status { get; }

        /// <summary>
        /// Gets the name of the world.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the IP address to connect to this world.
        /// </summary>
        string IpAddress { get; }

        /// <summary>
        /// Gets the port to connect to this world.
        /// </summary>
        ushort? Port { get; }
    }
}
