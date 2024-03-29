﻿// -----------------------------------------------------------------
// <copyright file="AStarSearchContext.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Plugins.PathFinding.AStar
{
    using System;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Pathfinding.Abstractions;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a search context for the AStar pathfinding.
    /// </summary>
    internal class AStarSearchContext : ISearchContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AStarSearchContext"/> class.
        /// </summary>
        /// <param name="searchId">The id of the search in progress.</param>
        /// <param name="map">A reference to the map instance.</param>
        /// <param name="forCreature">The creature on behalf of which the search is being performed.</param>
        /// <param name="considerAvoidsAsBlocking">Optional. A value indicating whether to consider the creature's avoid flags as completely blocking. Defaults to true.</param>
        /// <param name="targetDistance">Optional. A value to use for the target distance from the target node.</param>
        /// <param name="moveAway">Optional. A value indicating whether the current search is to move away.</param>
        /// <param name="excludeLocations">Optional. Locations to explicitly exclude as a valid goal in the search.</param>
        public AStarSearchContext(string searchId, IMap map, ICreature forCreature, bool considerAvoidsAsBlocking = true, int targetDistance = 1, bool moveAway = false, params Location[] excludeLocations)
        {
            searchId.ThrowIfNullOrWhiteSpace(nameof(searchId));
            map.ThrowIfNull(nameof(map));
            forCreature.ThrowIfNull(nameof(forCreature));

            this.SearchId = searchId;
            this.Map = map;
            this.MoveAway = moveAway;
            this.OnBehalfOfCreature = forCreature;
            this.ConsiderAvoidsAsBlocking = considerAvoidsAsBlocking;
            this.TargetDistance = targetDistance;
            this.ExcludeLocations = excludeLocations ?? Array.Empty<Location>();
        }

        /// <summary>
        /// Gets the location of the tile.
        /// </summary>
        public string SearchId { get; }

        /// <summary>
        /// Gets the map instance in use.
        /// </summary>
        public IMap Map { get; }

        /// <summary>
        /// Gets the creature on behalf of which this node calculates movement costs.
        /// </summary>
        public ICreature OnBehalfOfCreature { get; }

        /// <summary>
        /// Gets a value indicating whether to consider the creature's avoid flags as completely blocking.
        /// </summary>
        public bool ConsiderAvoidsAsBlocking { get; }

        /// <summary>
        /// Gets the target distance from the target node.
        /// </summary>
        public int TargetDistance { get; }

        /// <summary>
        /// Gets a value indicating whether the intention of the search is to move away from the goal.
        /// </summary>
        public bool MoveAway { get; }

        /// <summary>
        /// Gets a set of locations to explicitly exclude as a valid goal in the search.
        /// </summary>
        public Location[] ExcludeLocations { get; }
    }
}
