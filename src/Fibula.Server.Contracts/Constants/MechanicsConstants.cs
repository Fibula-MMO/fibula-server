﻿// -----------------------------------------------------------------
// <copyright file="MechanicsConstants.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Constants
{
    /// <summary>
    /// Static class that contains mechanics constants.
    /// </summary>
    public static class MechanicsConstants
    {
        /// <summary>
        /// A default value to use when calculating movement penalty.
        /// </summary>
        public const int DefaultGroundMovementPenaltyInMs = 150;

        /// <summary>
        /// The default maximum delay to introduce between a death ocurring the it's consequences (i.e. body dropping) happening.
        /// </summary>
        public const int DefaultDeathDelayMs = 2000;
    }
}
