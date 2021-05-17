// -----------------------------------------------------------------
// <copyright file="CharacterLoginInformation.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Contracts.Structures
{
    /// <summary>
    /// Structure with login information about character.
    /// </summary>
    public struct CharacterLoginInformation
    {
        /// <summary>
        /// Gets or sets the name of the character.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the world where this character lives.
        /// </summary>
        public string World { get; set; }

        /// <summary>
        /// Gets or sets the IP address bytes that the client must use to connect if loging in with this character.
        /// </summary>
        public byte[] Ip { get; set; }

        /// <summary>
        /// Gets or sets the port that the client must use to connect if loging in with this character.
        /// </summary>
        public ushort Port { get; set; }
    }
}
