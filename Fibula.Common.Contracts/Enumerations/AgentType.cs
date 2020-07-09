﻿// -----------------------------------------------------------------
// <copyright file="AgentType.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Common.Contracts.Enumerations
{
    /// <summary>
    /// Enumeration of the possible type of client agents.
    /// </summary>
    public enum AgentType : ushort
    {
        /// <summary>
        /// Undefined.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// The Linux client.
        /// </summary>
        Linux = 1,

        /// <summary>
        /// The Windows client.
        /// </summary>
        Windows = 2,

        /// <summary>
        /// The flash client.
        /// </summary>
        Flash = 3,
    }
}
