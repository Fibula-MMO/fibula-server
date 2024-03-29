﻿// -----------------------------------------------------------------
// <copyright file="DirectionExtensions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Extensions
{
    using Fibula.Definitions.Enumerations;

    /// <summary>
    /// Static class that contains extension methods for a <see cref="Direction"/>.
    /// </summary>
    public static class DirectionExtensions
    {
        /// <summary>
        /// Converts given direction into one of the four directions safe to send to the Tibia client.
        /// </summary>
        /// <param name="direction">The direction to convert.</param>
        /// <returns>The client-safe direction.</returns>
        public static Direction GetClientSafeDirection(this Direction direction)
        {
            switch (direction)
            {
                default:
                case Direction.South:
                    return Direction.South;

                case Direction.North:
                    return Direction.North;

                case Direction.East:
                case Direction.NorthEast:
                case Direction.SouthEast:
                    return Direction.East;

                case Direction.West:
                case Direction.NorthWest:
                case Direction.SouthWest:
                    return Direction.West;
            }
        }

        /// <summary>
        /// Determines if this direction is a diagonal one.
        /// </summary>
        /// <param name="direction">The direction to evaluate.</param>
        /// <returns>True if the direction is diagonal, false otherwise.</returns>
        public static bool IsDiagonal(this Direction direction)
        {
            switch (direction)
            {
                default:
                case Direction.South:
                case Direction.North:
                case Direction.East:
                case Direction.West:
                    return false;

                case Direction.NorthEast:
                case Direction.SouthEast:
                case Direction.NorthWest:
                case Direction.SouthWest:
                    return true;
            }
        }
    }
}
