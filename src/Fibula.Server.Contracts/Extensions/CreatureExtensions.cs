// -----------------------------------------------------------------
// <copyright file="CreatureExtensions.cs" company="2Dudes">
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
    using System;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Scheduling.Contracts.Abstractions;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Constants;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Static class that provides helper methods for creature mechanics.
    /// </summary>
    public static class CreatureExtensions
    {
        /// <summary>
        /// Calculates the step duration of a creature moving from a given tile in the given direction.
        /// </summary>
        /// <param name="creature">The creature that's moving.</param>
        /// <param name="fromTile">The tile which the creature is moving from.</param>
        /// <returns>The duration time of the step.</returns>
        public static TimeSpan CalculateStepDuration(this ICreature creature, ITile fromTile = null)
        {
            if (creature == null)
            {
                return TimeSpan.Zero;
            }

            var tilePenalty = fromTile?.Ground?.MovementPenalty ?? MechanicsConstants.DefaultGroundMovementPenaltyInMs;

            decimal totalPenalty = 1000 * tilePenalty;
            decimal stepSpeed = Math.Max(1u, creature.Speed);

            var durationInMs = (uint)(Math.Round(totalPenalty / stepSpeed, MidpointRounding.ToEven) * creature.LastMovementCostModifier);

            return TimeSpan.FromMilliseconds(durationInMs);
        }

        /// <summary>
        /// Checks if a given creature can see another creature.
        /// </summary>
        /// <param name="creature">The creature.</param>
        /// <param name="otherCreature">The creature to check against.</param>
        /// <returns>True if this creature can see the given creature, false otherwise.</returns>
        public static bool CanSee(this ICreature creature, ICreature otherCreature)
        {
            otherCreature.ThrowIfNull(nameof(otherCreature));

            // (!otherCreature.IsInvisible || this.CanSeeInvisible) &&
            return creature.CanSee(otherCreature.Location);
        }

        /// <summary>
        /// Checks if a given creature can see a given location.
        /// </summary>
        /// <param name="creature">The creature.</param>
        /// <param name="location">The location to check against.</param>
        /// <returns>True if this creature can see the given location, false otherwise.</returns>
        public static bool CanSee(this ICreature creature, Location location)
        {
            if (creature.Location.Z <= MapConstants.GroundSurfaceZ)
            {
                // we are on ground level or above (surface -> 0)
                // view is from surface -> 0
                if (location.Z > MapConstants.GroundSurfaceZ)
                {
                    return false;
                }
            }
            else if (creature.Location.Z > MapConstants.GroundSurfaceZ)
            {
                // we are underground (8 -> 15)
                // view is +/- 2 from the floor we stand on
                if (Math.Abs(creature.Location.Z - location.Z) > 2)
                {
                    return false;
                }
            }

            var offsetZ = creature.Location.Z - location.Z;
            var withinTopBorder = location.Y > creature.Location.Y - (MapConstants.DefaultWindowSizeY / 2) + offsetZ;
            var withinLeftBorder = location.X > creature.Location.X - (MapConstants.DefaultWindowSizeX / 2) + offsetZ;
            var withinRightBorder = location.X <= creature.Location.X + (MapConstants.DefaultWindowSizeX / 2) + offsetZ;
            var withinBottomBorder = location.Y <= creature.Location.Y + (MapConstants.DefaultWindowSizeY / 2) + offsetZ;

            if (withinTopBorder && withinLeftBorder && withinRightBorder && withinBottomBorder)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a given creature can push a given creature.
        /// </summary>
        /// <param name="creature">The creature.</param>
        /// <param name="otherCreature">The creature being pushed.</param>
        /// <returns>True if this creature can see the given location, false otherwise.</returns>
        public static bool CanPush(this ICreature creature, ICreature otherCreature)
        {
            if (creature == null || otherCreature == null)
            {
                return false;
            }

            // TODO: evaluate access/permissions, player vs monster, monster size/weight or some other metrics.
            return true;
        }

        /// <summary>
        /// Checks if the creature has an operation being tracked under the given identifier and outputs it if so.
        /// </summary>
        /// <param name="creature">The creature to check in.</param>
        /// <param name="identifier">The identifier of the operation to check for.</param>
        /// <param name="operation">The operation found, if any.</param>
        /// <returns>True if a tracked operation was found, and false otherwise.</returns>
        public static bool TryRetrieveTrackedOperation(this ICreature creature, string identifier, out IOperation operation)
        {
            creature.ThrowIfNull(nameof(creature));
            operation = creature.TrackedEvents.TryGetValue(identifier, out IEvent evt) ? evt as IOperation : null;

            return operation != null;
        }

        /// <summary>
        /// Gets the location in front of a creature.
        /// </summary>
        /// <param name="ofCreature">The creature from which to get the location in front of.</param>
        /// <returns>The location that's in front of a creature.</returns>
        public static Location LocationInFront(this ICreature ofCreature)
        {
            ofCreature.ThrowIfNull(nameof(ofCreature));

            return ofCreature.Direction switch
            {
                Direction.North => ofCreature.Location + new Location() { X = 0, Y = -1, Z = 0 },
                Direction.East => ofCreature.Location + new Location() { X = 1, Y = 0, Z = 0 },
                Direction.South => ofCreature.Location + new Location() { X = 0, Y = 1, Z = 0 },
                Direction.West => ofCreature.Location + new Location() { X = -1, Y = 0, Z = 0 },
                _ => ofCreature.Location,
            };
        }

        /// <summary>
        /// Gets a random direction adjacent to the creature.
        /// </summary>
        /// <param name="ofCreature">The creature from which to get the direction from.</param>
        /// <param name="includeDiagonals">A value indicating whether to pick from diagonal directions too.</param>
        /// <param name="rng">Optional. An instance of a pseudo-random number generator to use.</param>
        /// <returns>The direction that's adjacent to a creature.</returns>
        public static Direction RandomAdjacentDirection(this ICreature ofCreature, bool includeDiagonals = false, Random rng = null)
        {
            ofCreature.ThrowIfNull(nameof(ofCreature));

            if (rng == null)
            {
                rng = new Random();
            }

            var pickedLocation = ofCreature.RandomAdjacentLocation(includeDiagonals, rng);

            return ofCreature.Location.DirectionTo(pickedLocation, includeDiagonals);
        }

        /// <summary>
        /// Gets a random location adjacent to the creature.
        /// </summary>
        /// <param name="ofCreature">The creature from which to get the location from.</param>
        /// <param name="includeDiagonals">A value indicating whether to pick from diagonal locations too.</param>
        /// <param name="rng">Optional. An instance of a pseudo-random number generator to use.</param>
        /// <returns>The location that's adjacent to a creature.</returns>
        public static Location RandomAdjacentLocation(this ICreature ofCreature, bool includeDiagonals = false, Random rng = null)
        {
            ofCreature.ThrowIfNull(nameof(ofCreature));

            if (rng == null)
            {
                rng = new Random();
            }

            // Instead of picking the offset randomly and then checking for the edge case (middle),
            // we assign all different values to the adjacent locations and then picked from there.
            var pickedLocationOffset = includeDiagonals ? rng.Next(8) : rng.Next(4);

            return pickedLocationOffset switch
            {
                // Non-diagonals.
                0 => ofCreature.Location + new Location() { X = 0, Y = -1, Z = 0 },
                1 => ofCreature.Location + new Location() { X = 1, Y = 0, Z = 0 },
                2 => ofCreature.Location + new Location() { X = -1, Y = 0, Z = 0 },
                3 => ofCreature.Location + new Location() { X = 0, Y = 1, Z = 0 },

                // Diagonals.
                4 => ofCreature.Location + new Location() { X = -1, Y = -1, Z = 0 },
                5 => ofCreature.Location + new Location() { X = 1, Y = -1, Z = 0 },
                6 => ofCreature.Location + new Location() { X = -1, Y = 1, Z = 0 },
                _ => ofCreature.Location + new Location() { X = 1, Y = 1, Z = 0 },
            };
        }
    }
}
