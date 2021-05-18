// -----------------------------------------------------------------
// <copyright file="CreatureMovedNotification.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Enumerations;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Constants;
    using Fibula.Server.Contracts.Extensions;

    /// <summary>
    /// Class that represents a notification for when a creature has moved.
    /// </summary>
    public class CreatureMovedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureMovedNotification"/> class.
        /// </summary>
        /// <param name="targetPlayers">The target players of this notification.</param>
        /// <param name="map">A reference to the map, to get descriptions from.</param>
        /// <param name="movingCreature">The creature moving.</param>
        /// <param name="fromLocation">The location from which the creature is moving.</param>
        /// <param name="fromStackPos">The stack position from where the creature moving.</param>
        /// <param name="toLocation">The location to which the creature is moving.</param>
        /// <param name="toStackPos">The stack position to which the creature is moving.</param>
        /// <param name="wasTeleport">A value indicating whether this movement was a teleportation.</param>
        public CreatureMovedNotification(
            IEnumerable<IPlayer> targetPlayers,
            IMap map,
            ICreature movingCreature,
            Location fromLocation,
            byte fromStackPos,
            Location toLocation,
            byte toStackPos,
            bool wasTeleport)
            : base(targetPlayers)
        {
            var locationDiff = fromLocation - toLocation;

            this.Map = map;
            this.Creature = movingCreature;
            this.OldLocation = fromLocation;
            this.OldStackPosition = fromStackPos;
            this.NewLocation = toLocation;
            this.NewStackPosition = toStackPos;
            this.WasTeleport = wasTeleport || locationDiff.MaxValueIn3D > 1;
        }

        /// <summary>
        /// Gets a reference to the map.
        /// </summary>
        public IMap Map { get; }

        /// <summary>
        /// Gets the creature being added.
        /// </summary>
        public ICreature Creature { get; }

        /// <summary>
        /// Gets a value indicating whether this movement was a teleportation.
        /// </summary>
        public bool WasTeleport { get; }

        /// <summary>
        /// Gets the location from which the creature is moving.
        /// </summary>
        public Location OldLocation { get; }

        /// <summary>
        /// Gets the stack position from where the creature moving.
        /// </summary>
        public byte OldStackPosition { get; }

        /// <summary>
        /// Gets the stack position to which the creature is moving.
        /// </summary>
        public byte NewStackPosition { get; }

        /// <summary>
        /// Gets the location to which the creature is moving.
        /// </summary>
        public Location NewLocation { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            var packets = new List<IOutboundPacket>();

            if (this.Creature == player)
            {
                if (this.WasTeleport)
                {
                    if (this.OldStackPosition <= MapConstants.MaximumNumberOfThingsToDescribePerTile)
                    {
                        // Since this was described to the client before, we send a packet that lets them know the thing must be removed from that Tile's stack.
                        packets.Add(new RemoveAtLocationPacket(this.OldLocation, this.OldStackPosition));
                    }

                    // Then send the entire description at the new location.
                    var descriptionTiles = this.Map.DescribeAt(player, this.NewLocation);

                    packets.Add(new MapDescriptionPacket(player, this.NewLocation, descriptionTiles));

                    return packets;
                }

                if (this.OldLocation.Z == MapConstants.GroundSurfaceZ && this.NewLocation.Z > MapConstants.GroundSurfaceZ)
                {
                    if (this.OldStackPosition <= MapConstants.MaximumNumberOfThingsToDescribePerTile)
                    {
                        packets.Add(new RemoveAtLocationPacket(this.OldLocation, this.OldStackPosition));
                    }
                }
                else
                {
                    packets.Add(new CreatureMovedPacket(this.OldLocation, this.OldStackPosition, this.NewLocation));
                }

                // floor change down
                if (this.NewLocation.Z > this.OldLocation.Z)
                {
                    var windowStartLocation = new Location()
                    {
                        X = this.OldLocation.X - ((MapConstants.DefaultWindowSizeX / 2) - 1), // -8
                        Y = this.OldLocation.Y - ((MapConstants.DefaultWindowSizeY / 2) - 1), // -6
                        Z = this.NewLocation.Z,
                    };

                    IEnumerable<ITile> description;

                    // going from surface to underground
                    if (this.NewLocation.Z == 8)
                    {
                        // Client already has the two floors above (6 and 7), so it needs 8 (new current), and 2 below.
                        description = this.Map.DescribeWindow(
                            player,
                            (ushort)windowStartLocation.X,
                            (ushort)windowStartLocation.Y,
                            this.NewLocation.Z,
                            (sbyte)(this.NewLocation.Z + 2),
                            MapConstants.DefaultWindowSizeX,
                            MapConstants.DefaultWindowSizeY,
                            -1);
                    }

                    // going further down underground; watch for world's deepest floor (hardcoded for now).
                    else if (this.NewLocation.Z > 8 && this.NewLocation.Z < 14)
                    {
                        // Client already has all floors needed except the new deepest floor, so it needs the 2th floor below the current.
                        description = this.Map.DescribeWindow(
                            player,
                            (ushort)windowStartLocation.X,
                            (ushort)windowStartLocation.Y,
                            (sbyte)(this.NewLocation.Z + 2),
                            (sbyte)(this.NewLocation.Z + 2),
                            MapConstants.DefaultWindowSizeX,
                            MapConstants.DefaultWindowSizeY,
                            -3);
                    }

                    // going down but still above surface, so client has all floors.
                    else
                    {
                        description = Enumerable.Empty<ITile>();
                    }

                    packets.Add(new MapPartialDescriptionPacket(OutboundPacketType.FloorChangeDown, player, description));

                    // moving down a floor makes us out of sync, include east and south
                    var eastDescriptionTiles = this.EastSliceDescription(
                            player,
                            this.OldLocation.X - this.NewLocation.X,
                            this.OldLocation.Y - this.NewLocation.Y + this.OldLocation.Z - this.NewLocation.Z);

                    packets.Add(new MapPartialDescriptionPacket(OutboundPacketType.MapSliceEast, player, eastDescriptionTiles));

                    var southDescriptionTiles = this.SouthSliceDescription(player, this.OldLocation.Y - this.NewLocation.Y);

                    packets.Add(new MapPartialDescriptionPacket(OutboundPacketType.MapSliceSouth, player, southDescriptionTiles));
                }

                // floor change up
                else if (this.NewLocation.Z < this.OldLocation.Z)
                {
                    var windowStartLocation = new Location()
                    {
                        X = this.OldLocation.X - ((MapConstants.DefaultWindowSizeX / 2) - 1), // -8
                        Y = this.OldLocation.Y - ((MapConstants.DefaultWindowSizeY / 2) - 1), // -6
                        Z = this.NewLocation.Z,
                    };

                    IEnumerable<ITile> description;

                    // going to surface
                    if (this.NewLocation.Z == MapConstants.GroundSurfaceZ)
                    {
                        // Client already has the first two above-the-ground floors (6 and 7), so it needs 0-5 above.
                        description = this.Map.DescribeWindow(
                            player,
                            (ushort)windowStartLocation.X,
                            (ushort)windowStartLocation.Y,
                            5,
                            0,
                            MapConstants.DefaultWindowSizeX,
                            MapConstants.DefaultWindowSizeY,
                            3);
                    }

                    // going up but still underground
                    else if (this.NewLocation.Z > MapConstants.GroundSurfaceZ)
                    {
                        // Client already has all floors needed except the new highest floor, so it needs the 2th floor above the current.
                        description = this.Map.DescribeWindow(
                            player,
                            (ushort)windowStartLocation.X,
                            (ushort)windowStartLocation.Y,
                            (sbyte)(this.NewLocation.Z - 2),
                            (sbyte)(this.NewLocation.Z - 2),
                            MapConstants.DefaultWindowSizeX,
                            MapConstants.DefaultWindowSizeY,
                            3);
                    }

                    // already above surface, so client has all floors.
                    else
                    {
                        description = Enumerable.Empty<ITile>();
                    }

                    packets.Add(new MapPartialDescriptionPacket(OutboundPacketType.FloorChangeUp, player, description));

                    // moving up a floor up makes us out of sync, include west and north
                    var westDescriptionTiles = this.WestSliceDescription(
                            player,
                            this.OldLocation.X - this.NewLocation.X,
                            this.OldLocation.Y - this.NewLocation.Y + this.OldLocation.Z - this.NewLocation.Z);

                    packets.Add(new MapPartialDescriptionPacket(OutboundPacketType.MapSliceWest, player, westDescriptionTiles));

                    var northDescriptionTiles = this.NorthSliceDescription(player, this.OldLocation.Y - this.NewLocation.Y);

                    packets.Add(new MapPartialDescriptionPacket(OutboundPacketType.MapSliceNorth, player, northDescriptionTiles));
                }

                if (this.OldLocation.Y > this.NewLocation.Y)
                {
                    // Creature is moving north, so we need to send the additional north bytes.
                    var northDescriptionTiles = this.NorthSliceDescription(player);

                    packets.Add(new MapPartialDescriptionPacket(OutboundPacketType.MapSliceNorth, player, northDescriptionTiles));
                }
                else if (this.OldLocation.Y < this.NewLocation.Y)
                {
                    // Creature is moving south, so we need to send the additional south bytes.
                    var southDescriptionTiles = this.SouthSliceDescription(player);

                    packets.Add(new MapPartialDescriptionPacket(OutboundPacketType.MapSliceSouth, player, southDescriptionTiles));
                }

                if (this.OldLocation.X < this.NewLocation.X)
                {
                    // Creature is moving east, so we need to send the additional east bytes.
                    var eastDescriptionTiles = this.EastSliceDescription(player);

                    packets.Add(new MapPartialDescriptionPacket(OutboundPacketType.MapSliceEast, player, eastDescriptionTiles));
                }
                else if (this.OldLocation.X > this.NewLocation.X)
                {
                    // Creature is moving west, so we need to send the additional west bytes.
                    var westDescriptionTiles = this.WestSliceDescription(player);

                    packets.Add(new MapPartialDescriptionPacket(OutboundPacketType.MapSliceWest, player, westDescriptionTiles));
                }
            }
            else if (player.CanSee(this.OldLocation) && player.CanSee(this.NewLocation))
            {
                if (player.CanSee(this.Creature))
                {
                    if (this.WasTeleport || (this.OldLocation.Z == MapConstants.GroundSurfaceZ && this.NewLocation.Z > MapConstants.GroundSurfaceZ) || this.OldStackPosition > 9)
                    {
                        if (this.OldStackPosition <= MapConstants.MaximumNumberOfThingsToDescribePerTile)
                        {
                            packets.Add(new RemoveAtLocationPacket(this.OldLocation, this.OldStackPosition));
                        }

                        packets.Add(new AddCreaturePacket(player, this.Creature));
                    }
                    else
                    {
                        packets.Add(new CreatureMovedPacket(this.OldLocation, this.OldStackPosition, this.NewLocation));
                    }
                }
            }
            else if (player.CanSee(this.OldLocation) && !player.CanSee(this.Creature))
            {
                if (this.OldStackPosition <= MapConstants.MaximumNumberOfThingsToDescribePerTile)
                {
                    packets.Add(new RemoveAtLocationPacket(this.OldLocation, this.OldStackPosition));
                }
            }
            else if (player.CanSee(this.NewLocation) && player.CanSee(this.Creature))
            {
                if (this.NewStackPosition <= MapConstants.MaximumNumberOfThingsToDescribePerTile)
                {
                    packets.Add(new AddCreaturePacket(player, this.Creature));
                }
            }

            if (this.WasTeleport)
            {
                packets.Add(new MagicEffectPacket(this.NewLocation, AnimatedEffect.BubbleBlue));
            }

            return packets;
        }

        private IEnumerable<ITile> NorthSliceDescription(IPlayer player, int floorChangeOffset = 0)
        {
            // A = old location, B = new location.
            //
            //       |------ MapConstants.DefaultWindowSizeX = 18 ------|
            //                           as seen by A
            //       x  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .     ---
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  B  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  A  .  .  .  .  .  .  .  .  .      | MapConstants.DefaultWindowSizeY = 14
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      | as seen by A
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .     ---
            //
            // x = target start of window (~) to refresh.
            var windowStartLocation = new Location()
            {
                // -8
                X = this.OldLocation.X - ((MapConstants.DefaultWindowSizeX / 2) - 1),

                // -6
                Y = this.NewLocation.Y - ((MapConstants.DefaultWindowSizeY / 2) - 1 - floorChangeOffset),

                Z = this.NewLocation.Z,
            };

            return this.Map.DescribeWindow(
                    player,
                    (ushort)windowStartLocation.X,
                    (ushort)windowStartLocation.Y,
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Max(MapConstants.MinimumLevelZ, this.NewLocation.Z - 2) : 7),
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Min(MapConstants.MaximumLevelZ, this.NewLocation.Z + 2) : 0),
                    MapConstants.DefaultWindowSizeX,
                    1);
        }

        private IEnumerable<ITile> SouthSliceDescription(IPlayer player, int floorChangeOffset = 0)
        {
            // A = old location, B = new location
            //
            //       |------ MapConstants.DefaultWindowSizeX = 18 ------|
            //                           as seen by A
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .     ---
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  A  .  .  .  .  .  .  .  .  .      | MapConstants.DefaultWindowSizeY = 14
            //       .  .  .  .  .  .  .  .  B  .  .  .  .  .  .  .  .  .      | as seen by A
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .      |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .     ---
            //       x  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~  ~
            //
            // x = target start of window (~) to refresh.
            var windowStartLocation = new Location()
            {
                // -8
                X = this.OldLocation.X - ((MapConstants.DefaultWindowSizeX / 2) - 1),

                // +7
                Y = this.NewLocation.Y + (MapConstants.DefaultWindowSizeY / 2) + floorChangeOffset,

                Z = this.NewLocation.Z,
            };

            return this.Map.DescribeWindow(
                    player,
                    (ushort)windowStartLocation.X,
                    (ushort)windowStartLocation.Y,
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Max(MapConstants.MinimumLevelZ, this.NewLocation.Z - 2) : 7),
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Min(MapConstants.MaximumLevelZ, this.NewLocation.Z + 2) : 0),
                    MapConstants.DefaultWindowSizeX,
                    1);
        }

        private IEnumerable<ITile> EastSliceDescription(IPlayer player, int floorChangeOffsetX = 0, int floorChangeOffsetY = 0)
        {
            // A = old location, B = new location
            //
            //       |------ MapConstants.DefaultWindowSizeX = 18 ------|
            //                           as seen by A
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  x  ---
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ~   |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ~   |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ~   |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ~   |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ~   |
            //       .  .  .  .  .  .  .  .  A  B  .  .  .  .  .  .  .  .  ~   | MapConstants.DefaultWindowSizeY = 14
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ~   | as seen by A
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ~   |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ~   |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ~   |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ~   |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ~   |
            //       .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ~  ---
            //
            // x = target start of window (~) to refresh.
            var windowStartLocation = new Location()
            {
                // +9
                X = this.NewLocation.X + (MapConstants.DefaultWindowSizeX / 2) + floorChangeOffsetX,

                // -6
                Y = this.NewLocation.Y - ((MapConstants.DefaultWindowSizeY / 2) - 1) + floorChangeOffsetY,

                Z = this.NewLocation.Z,
            };

            return this.Map.DescribeWindow(
                    player,
                    (ushort)windowStartLocation.X,
                    (ushort)windowStartLocation.Y,
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Max(MapConstants.MinimumLevelZ, this.NewLocation.Z - 2) : 7),
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Min(MapConstants.MaximumLevelZ, this.NewLocation.Z + 2) : 0),
                    1,
                    MapConstants.DefaultWindowSizeY);
        }

        private IEnumerable<ITile> WestSliceDescription(IPlayer player, int floorChangeOffsetX = 0, int floorChangeOffsetY = 0)
        {
            // A = old location, B = new location
            //
            //          |------ MapConstants.DefaultWindowSizeX = 18 ------|
            //                           as seen by A
            //       x  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ---
            //       ~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .   |
            //       ~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .   |
            //       ~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .   |
            //       ~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .   |
            //       ~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .   |
            //       ~  .  .  .  .  .  .  .  B  A  .  .  .  .  .  .  .  .  .   | MapConstants.DefaultWindowSizeY = 14
            //       ~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .   | as seen by A
            //       ~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .   |
            //       ~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .   |
            //       ~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .   |
            //       ~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .   |
            //       ~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .   |
            //       ~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  ---
            //
            // x = target start of window (~) to refresh.
            var windowStartLocation = new Location()
            {
                // -8
                X = this.NewLocation.X - ((MapConstants.DefaultWindowSizeX / 2) - 1) + floorChangeOffsetX,

                // -6
                Y = this.NewLocation.Y - ((MapConstants.DefaultWindowSizeY / 2) - 1) + floorChangeOffsetY,

                Z = this.NewLocation.Z,
            };

            return this.Map.DescribeWindow(
                    player,
                    (ushort)windowStartLocation.X,
                    (ushort)windowStartLocation.Y,
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Max(MapConstants.MinimumLevelZ, this.NewLocation.Z - 2) : 7),
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Min(MapConstants.MaximumLevelZ, this.NewLocation.Z + 2) : 0),
                    1,
                    MapConstants.DefaultWindowSizeY);
        }
    }
}
