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
    using System.Buffers;
    using System.Collections.Generic;
    using System.Threading.Tasks.Dataflow;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Constants;
    using Fibula.Server.Contracts.Enumerations;

    /// <summary>
    /// Class that represents a notification for when a creature has moved.
    /// </summary>
    public class CreatureMovedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureMovedNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="creatureId">The id of the creature moving.</param>
        /// <param name="fromLocation">The location from which the creature is moving.</param>
        /// <param name="fromStackPos">The stack position from where the creature moving.</param>
        /// <param name="toLocation">The location to which the creature is moving.</param>
        /// <param name="toStackPos">The stack position to which the creature is moving.</param>
        /// <param name="wasTeleport">A value indicating whether this movement was a teleportation.</param>
        public CreatureMovedNotification(
            Func<IEnumerable<IPlayer>> findTargetPlayers,
            uint creatureId,
            Location fromLocation,
            byte fromStackPos,
            Location toLocation,
            byte toStackPos,
            bool wasTeleport)
            : base(findTargetPlayers)
        {
            var locationDiff = fromLocation - toLocation;

            this.CreatureId = creatureId;
            this.OldLocation = fromLocation;
            this.OldStackPosition = fromStackPos;
            this.NewLocation = toLocation;
            this.NewStackPosition = toStackPos;
            this.WasTeleport = wasTeleport || locationDiff.MaxValueIn3D > 1;
        }

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
        /// Gets the id of the creature moving.
        /// </summary>
        public uint CreatureId { get; }

        /// <summary>
        /// Finalizes the notification in preparation to it being sent.
        /// </summary>
        /// <param name="context">The context of this notification.</param>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>True if the notification was posted successfuly, and false otherwise.</returns>
        public override bool Post(INotificationContext context, IPlayer player)
        {
            if (!(context.Buffer is ITargetBlock<GameNotification> targetBuffer))
            {
                return false;
            }

            var creature = context.CreatureFinder.FindCreatureById(this.CreatureId);

            if (this.CreatureId == player.Id)
            {
                if (this.WasTeleport)
                {
                    if (this.OldStackPosition <= MapConstants.MaximumNumberOfThingsToDescribePerTile)
                    {
                        // Since this was described to the client before, we send a packet that lets them know the thing must be removed from that Tile's stack.
                        targetBuffer.Post(
                            new GameNotification()
                            {
                                RemoveAtLocation = new RemoveAtLocation()
                                {
                                    Location = new Common.Contracts.Grpc.Location()
                                    {
                                        X = (ulong)this.OldLocation.X,
                                        Y = (ulong)this.OldLocation.Y,
                                        Z = (uint)this.OldLocation.Z,
                                    },
                                    Index = this.OldStackPosition,
                                },
                            });
                    }

                    // Then send the entire description at the new location.
                    var (descriptionMetadata, descriptionBytes) = context.MapDescriptor.DescribeAt(player, this.NewLocation);

                    var mapDescription = new Common.Contracts.Grpc.MapDescription()
                    {
                        Description = ByteString.CopyFrom(descriptionBytes.ToArray()),
                    };

                    if (descriptionMetadata.ContainsKey(IMapDescriptor.CreatureIdsToLearnMetadataKeyName))
                    {
                        mapDescription.CreaturesBeingAdded.AddRange((IEnumerable<uint>)descriptionMetadata[IMapDescriptor.CreatureIdsToLearnMetadataKeyName]);
                    }

                    if (descriptionMetadata.ContainsKey(IMapDescriptor.CreatureIdsToForgetMetadataKeyName))
                    {
                        mapDescription.CreaturesBeingRemoved.AddRange((IEnumerable<uint>)descriptionMetadata[IMapDescriptor.CreatureIdsToForgetMetadataKeyName]);
                    }

                    return targetBuffer.Post(
                            new GameNotification()
                            {
                                MapDescriptionFull = new MapDescriptionFull()
                                {
                                    Origin = new Common.Contracts.Grpc.Location()
                                    {
                                        X = (ulong)this.NewLocation.X,
                                        Y = (ulong)this.NewLocation.Y,
                                        Z = (uint)this.NewLocation.Z,
                                    },
                                    Description = mapDescription,
                                },
                            });
                }

                if (this.OldLocation.Z == 7 && this.NewLocation.Z > 7)
                {
                    if (this.OldStackPosition <= MapConstants.MaximumNumberOfThingsToDescribePerTile)
                    {
                        targetBuffer.Post(
                            new GameNotification()
                            {
                                RemoveAtLocation = new RemoveAtLocation()
                                {
                                    Location = new Common.Contracts.Grpc.Location()
                                    {
                                        X = (ulong)this.OldLocation.X,
                                        Y = (ulong)this.OldLocation.Y,
                                        Z = (uint)this.OldLocation.Z,
                                    },
                                    Index = this.OldStackPosition,
                                },
                            });
                    }
                }
                else
                {
                    targetBuffer.Post(
                        new GameNotification()
                        {
                            CreatureMoved = new CreatureMoved()
                            {
                                FromLocation = new Common.Contracts.Grpc.Location()
                                {
                                    X = (ulong)this.OldLocation.X,
                                    Y = (ulong)this.OldLocation.Y,
                                    Z = (uint)this.OldLocation.Z,
                                },
                                FromIndex = this.OldStackPosition,
                                ToLocation = new Common.Contracts.Grpc.Location()
                                {
                                    X = (ulong)this.NewLocation.X,
                                    Y = (ulong)this.NewLocation.Y,
                                    Z = (uint)this.NewLocation.Z,
                                },
                            },
                        });
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

                    (IDictionary<string, object> Metadata, ReadOnlySequence<byte> Data) description;

                    // going from surface to underground
                    if (this.NewLocation.Z == 8)
                    {
                        // Client already has the two floors above (6 and 7), so it needs 8 (new current), and 2 below.
                        description = context.MapDescriptor.DescribeWindow(
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
                        description = context.MapDescriptor.DescribeWindow(
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
                        description = (new Dictionary<string, object>(), ReadOnlySequence<byte>.Empty);
                    }

                    targetBuffer.Post(
                        new GameNotification()
                        {
                            MapDescriptionPartial = new MapDescriptionPartial()
                            {
                                Type = (uint)MapDescriptionType.BottomAndRight,
                                Description = new Common.Contracts.Grpc.MapDescription()
                                {
                                    Description = ByteString.CopyFrom(description.Data.ToArray()),
                                },
                            },
                        });

                    // moving down a floor makes us out of sync, include east and south
                    var (eastDescriptionMetadata, eastDescriptionBytes) = this.EastSliceDescription(
                            context,
                            player,
                            this.OldLocation.X - this.NewLocation.X,
                            this.OldLocation.Y - this.NewLocation.Y + this.OldLocation.Z - this.NewLocation.Z);

                    targetBuffer.Post(
                        new GameNotification()
                        {
                            MapDescriptionPartial = new MapDescriptionPartial()
                            {
                                Type = (uint)MapDescriptionType.RightOnly,
                                Description = new Common.Contracts.Grpc.MapDescription()
                                {
                                    Description = ByteString.CopyFrom(eastDescriptionBytes.ToArray()),
                                },
                            },
                        });

                    var (southDescriptionMetadata, southDescriptionBytes) = this.SouthSliceDescription(context, player, this.OldLocation.Y - this.NewLocation.Y);

                    targetBuffer.Post(
                        new GameNotification()
                        {
                            MapDescriptionPartial = new MapDescriptionPartial()
                            {
                                Type = (uint)MapDescriptionType.BottomOnly,
                                Description = new Common.Contracts.Grpc.MapDescription()
                                {
                                    Description = ByteString.CopyFrom(southDescriptionBytes.ToArray()),
                                },
                            },
                        });
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

                    (IDictionary<string, object> Metadata, ReadOnlySequence<byte> Data) description;

                    // going to surface
                    if (this.NewLocation.Z == 7)
                    {
                        // Client already has the first two above-the-ground floors (6 and 7), so it needs 0-5 above.
                        description = context.MapDescriptor.DescribeWindow(
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
                    else if (this.NewLocation.Z > 7)
                    {
                        // Client already has all floors needed except the new highest floor, so it needs the 2th floor above the current.
                        description = context.MapDescriptor.DescribeWindow(
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
                        description = (new Dictionary<string, object>(), ReadOnlySequence<byte>.Empty);
                    }

                    targetBuffer.Post(
                        new GameNotification()
                        {
                            MapDescriptionPartial = new MapDescriptionPartial()
                            {
                                Type = (uint)MapDescriptionType.LeftAndTop,
                                Description = new Common.Contracts.Grpc.MapDescription()
                                {
                                    Description = ByteString.CopyFrom(description.Data.ToArray()),
                                },
                            },
                        });

                    // moving up a floor up makes us out of sync, include west and north
                    var (westDescriptionMetadata, westDescriptionBytes) = this.WestSliceDescription(
                            context,
                            player,
                            this.OldLocation.X - this.NewLocation.X,
                            this.OldLocation.Y - this.NewLocation.Y + this.OldLocation.Z - this.NewLocation.Z);

                    targetBuffer.Post(
                        new GameNotification()
                        {
                            MapDescriptionPartial = new MapDescriptionPartial()
                            {
                                Type = (uint)MapDescriptionType.LeftOnly,
                                Description = new Common.Contracts.Grpc.MapDescription()
                                {
                                    Description = ByteString.CopyFrom(westDescriptionBytes.ToArray()),
                                },
                            },
                        });

                    var (northDescriptionMetadata, northDescriptionBytes) = this.NorthSliceDescription(context, player, this.OldLocation.Y - this.NewLocation.Y);

                    targetBuffer.Post(
                        new GameNotification()
                        {
                            MapDescriptionPartial = new MapDescriptionPartial()
                            {
                                Type = (uint)MapDescriptionType.TopOnly,
                                Description = new Common.Contracts.Grpc.MapDescription()
                                {
                                    Description = ByteString.CopyFrom(northDescriptionBytes.ToArray()),
                                },
                            },
                        });
                }

                if (this.OldLocation.Y > this.NewLocation.Y)
                {
                    // Creature is moving north, so we need to send the additional north bytes.
                    var (northDescriptionMetadata, northDescriptionBytes) = this.NorthSliceDescription(context, player);

                    targetBuffer.Post(
                        new GameNotification()
                        {
                            MapDescriptionPartial = new MapDescriptionPartial()
                            {
                                Type = (uint)MapDescriptionType.TopOnly,
                                Description = new Common.Contracts.Grpc.MapDescription()
                                {
                                    Description = ByteString.CopyFrom(northDescriptionBytes.ToArray()),
                                },
                            },
                        });
                }
                else if (this.OldLocation.Y < this.NewLocation.Y)
                {
                    // Creature is moving south, so we need to send the additional south bytes.
                    var (southDescriptionMetadata, southDescriptionBytes) = this.SouthSliceDescription(context, player);

                    targetBuffer.Post(
                        new GameNotification()
                        {
                            MapDescriptionPartial = new MapDescriptionPartial()
                            {
                                Type = (uint)MapDescriptionType.BottomOnly,
                                Description = new Common.Contracts.Grpc.MapDescription()
                                {
                                    Description = ByteString.CopyFrom(southDescriptionBytes.ToArray()),
                                },
                            },
                        });
                }

                if (this.OldLocation.X < this.NewLocation.X)
                {
                    // Creature is moving east, so we need to send the additional east bytes.
                    var (eastDescriptionMetadata, eastDescriptionBytes) = this.EastSliceDescription(context, player);

                    targetBuffer.Post(
                        new GameNotification()
                        {
                            MapDescriptionPartial = new MapDescriptionPartial()
                            {
                                Type = (uint)MapDescriptionType.RightOnly,
                                Description = new Common.Contracts.Grpc.MapDescription()
                                {
                                    Description = ByteString.CopyFrom(eastDescriptionBytes.ToArray()),
                                },
                            },
                        });
                }
                else if (this.OldLocation.X > this.NewLocation.X)
                {
                    // Creature is moving west, so we need to send the additional west bytes.
                    var (westDescriptionMetadata, westDescriptionBytes) = this.WestSliceDescription(context, player);

                    targetBuffer.Post(
                        new GameNotification()
                        {
                            MapDescriptionPartial = new MapDescriptionPartial()
                            {
                                Type = (uint)MapDescriptionType.LeftOnly,
                                Description = new Common.Contracts.Grpc.MapDescription()
                                {
                                    Description = ByteString.CopyFrom(westDescriptionBytes.ToArray()),
                                },
                            },
                        });
                }
            }
            else if (player.CanSee(this.OldLocation) && player.CanSee(this.NewLocation))
            {
                if (player.CanSee(creature))
                {
                    if (this.WasTeleport || (this.OldLocation.Z == 7 && this.NewLocation.Z > 7) || this.OldStackPosition > 9)
                    {
                        if (this.OldStackPosition <= MapConstants.MaximumNumberOfThingsToDescribePerTile)
                        {
                            targetBuffer.Post(
                                new GameNotification()
                                {
                                    RemoveAtLocation = new RemoveAtLocation()
                                    {
                                        Location = new Common.Contracts.Grpc.Location()
                                        {
                                            X = (ulong)this.OldLocation.X,
                                            Y = (ulong)this.OldLocation.Y,
                                            Z = (uint)this.OldLocation.Z,
                                        },
                                        Index = this.OldStackPosition,
                                    },
                                });
                        }

                        targetBuffer.Post(
                            new GameNotification()
                            {
                                AddCreature = new AddCreature()
                                {
                                    AtLocation = new Common.Contracts.Grpc.Location()
                                    {
                                        X = (ulong)creature.Location.X,
                                        Y = (ulong)creature.Location.Y,
                                        Z = (uint)creature.Location.Z,
                                    },
                                    Name = creature.Name,
                                    Speed = creature.Speed,
                                    HitpointsStat = new Common.Contracts.Grpc.Stat()
                                    {
                                        Name = CreatureStat.HitPoints.ToString(),
                                        Current = creature.Stats[CreatureStat.HitPoints].Current,
                                        Maximum = creature.Stats[CreatureStat.HitPoints].Maximum,
                                        Percent = creature.Stats[CreatureStat.HitPoints].Percent,
                                    },
                                    Light = new CreatureLight()
                                    {
                                        CreatureId = creature.Id,
                                        Level = creature.EmittedLightLevel,
                                        Color = creature.EmittedLightColor,
                                    },
                                    CreatureId = creature.Id,
                                    Direction = (uint)creature.Direction,
                                    Outfit = new Common.Contracts.Grpc.Outfit()
                                    {
                                        Id = creature.Outfit.Id,
                                        LookAlikeId = creature.Outfit.ItemIdLookAlike,
                                        Head = creature.Outfit.Head,
                                        Body = creature.Outfit.Body,
                                        Legs = creature.Outfit.Legs,
                                        Feet = creature.Outfit.Feet,
                                    },
                                },
                            });
                    }
                    else
                    {
                        targetBuffer.Post(
                            new GameNotification()
                            {
                                CreatureMoved = new CreatureMoved()
                                {
                                    FromLocation = new Common.Contracts.Grpc.Location()
                                    {
                                        X = (ulong)this.OldLocation.X,
                                        Y = (ulong)this.OldLocation.Y,
                                        Z = (uint)this.OldLocation.Z,
                                    },
                                    FromIndex = this.OldStackPosition,
                                    ToLocation = new Common.Contracts.Grpc.Location()
                                    {
                                        X = (ulong)this.NewLocation.X,
                                        Y = (ulong)this.NewLocation.Y,
                                        Z = (uint)this.NewLocation.Z,
                                    },
                                },
                            });
                    }
                }
            }
            else if (player.CanSee(this.OldLocation) && !player.CanSee(creature))
            {
                if (this.OldStackPosition <= MapConstants.MaximumNumberOfThingsToDescribePerTile)
                {
                    targetBuffer.Post(
                        new GameNotification()
                        {
                            RemoveAtLocation = new RemoveAtLocation()
                            {
                                Location = new Common.Contracts.Grpc.Location()
                                {
                                    X = (ulong)this.OldLocation.X,
                                    Y = (ulong)this.OldLocation.Y,
                                    Z = (uint)this.OldLocation.Z,
                                },
                                Index = this.OldStackPosition,
                            },
                        });
                }
            }
            else if (player.CanSee(this.NewLocation) && player.CanSee(creature))
            {
                if (this.NewStackPosition <= MapConstants.MaximumNumberOfThingsToDescribePerTile)
                {
                    targetBuffer.Post(
                        new GameNotification()
                        {
                            AddCreature = new AddCreature()
                            {
                                AtLocation = new Common.Contracts.Grpc.Location()
                                {
                                    X = (ulong)creature.Location.X,
                                    Y = (ulong)creature.Location.Y,
                                    Z = (uint)creature.Location.Z,
                                },
                                Name = creature.Name,
                                Speed = creature.Speed,
                                HitpointsStat = new Common.Contracts.Grpc.Stat()
                                {
                                     Name = CreatureStat.HitPoints.ToString(),
                                     Current = creature.Stats[CreatureStat.HitPoints].Current,
                                     Maximum = creature.Stats[CreatureStat.HitPoints].Maximum,
                                     Percent = creature.Stats[CreatureStat.HitPoints].Percent,
                                },
                                Light = new CreatureLight()
                                {
                                  CreatureId = creature.Id,
                                  Level = creature.EmittedLightLevel,
                                  Color = creature.EmittedLightColor,
                                },
                                CreatureId = creature.Id,
                                Direction = (uint)creature.Direction,
                                Outfit = new Common.Contracts.Grpc.Outfit()
                                {
                                    Id = creature.Outfit.Id,
                                    LookAlikeId = creature.Outfit.ItemIdLookAlike,
                                    Head = creature.Outfit.Head,
                                    Body = creature.Outfit.Body,
                                    Legs = creature.Outfit.Legs,
                                    Feet = creature.Outfit.Feet,
                                },
                            },
                        });
                }
            }

            if (this.WasTeleport)
            {
                return targetBuffer.Post(
                        new GameNotification()
                        {
                            MagicEffect = new MagicEffect()
                            {
                                Effect = (uint)AnimatedEffect.BubbleBlue,
                                Location = new Common.Contracts.Grpc.Location()
                                {
                                    X = (ulong)this.NewLocation.X,
                                    Y = (ulong)this.NewLocation.Y,
                                    Z = (uint)this.NewLocation.Z,
                                },
                            },
                        });
            }

            return true;
        }

        private (IDictionary<string, object> descriptionMetadata, ReadOnlySequence<byte> descriptionData) NorthSliceDescription(INotificationContext notificationContext, IPlayer player, int floorChangeOffset = 0)
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

            return notificationContext.MapDescriptor.DescribeWindow(
                    player,
                    (ushort)windowStartLocation.X,
                    (ushort)windowStartLocation.Y,
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Max(0, this.NewLocation.Z - 2) : 7),
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Min(15, this.NewLocation.Z + 2) : 0),
                    MapConstants.DefaultWindowSizeX,
                    1);
        }

        private (IDictionary<string, object> descriptionMetadata, ReadOnlySequence<byte> descriptionData) SouthSliceDescription(INotificationContext notificationContext, IPlayer player, int floorChangeOffset = 0)
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

            return notificationContext.MapDescriptor.DescribeWindow(
                    player,
                    (ushort)windowStartLocation.X,
                    (ushort)windowStartLocation.Y,
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Max(0, this.NewLocation.Z - 2) : 7),
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Min(15, this.NewLocation.Z + 2) : 0),
                    MapConstants.DefaultWindowSizeX,
                    1);
        }

        private (IDictionary<string, object> descriptionMetadata, ReadOnlySequence<byte> descriptionData) EastSliceDescription(INotificationContext notificationContext, IPlayer player, int floorChangeOffsetX = 0, int floorChangeOffsetY = 0)
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

            return notificationContext.MapDescriptor.DescribeWindow(
                    player,
                    (ushort)windowStartLocation.X,
                    (ushort)windowStartLocation.Y,
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Max(0, this.NewLocation.Z - 2) : 7),
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Min(15, this.NewLocation.Z + 2) : 0),
                    1,
                    MapConstants.DefaultWindowSizeY);
        }

        private (IDictionary<string, object> descriptionMetadata, ReadOnlySequence<byte> descriptionData) WestSliceDescription(INotificationContext notificationContext, IPlayer player, int floorChangeOffsetX = 0, int floorChangeOffsetY = 0)
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

            return notificationContext.MapDescriptor.DescribeWindow(
                    player,
                    (ushort)windowStartLocation.X,
                    (ushort)windowStartLocation.Y,
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Max(0, this.NewLocation.Z - 2) : 7),
                    (sbyte)(this.NewLocation.IsUnderground ? Math.Min(15, this.NewLocation.Z + 2) : 0),
                    1,
                    MapConstants.DefaultWindowSizeY);
        }
    }
}
