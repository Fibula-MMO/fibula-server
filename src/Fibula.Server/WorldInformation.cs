// -----------------------------------------------------------------
// <copyright file="WorldInformation.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Mechanics
{
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Enumerations;

    /// <summary>
    /// Class that implements an <see cref="IWorldInformation"/> model.
    /// </summary>
    public class WorldInformation : IWorldInformation
    {
        /// <summary>
        /// Gets or sets the id of the game world.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the world.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the IP address to connect to this world.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the port to connect to this world.
        /// </summary>
        public ushort? Port { get; set; }

        /// <summary>
        /// Gets or sets the game world's light color.
        /// </summary>
        public byte LightColor { get; set; }

        /// <summary>
        /// Gets or sets the game world's light level.
        /// </summary>
        public byte LightLevel { get; set; }

        /// <summary>
        /// Gets or sets the game world's state.
        /// </summary>
        public WorldState Status { get; set; }
    }
}
