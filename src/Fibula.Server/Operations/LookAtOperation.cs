﻿// -----------------------------------------------------------------
// <copyright file="LookAtOperation.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Operations
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Definitions.Flags;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Extensions;
    using Fibula.Server.Notifications;
    using Fibula.Utilities.Common.Extensions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents an event for a thing description.
    /// </summary>
    public class LookAtOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LookAtOperation"/> class.
        /// </summary>
        /// <param name="thingId">The id of the thing to describe.</param>
        /// <param name="location">The location where the thing to describe is.</param>
        /// <param name="stackPosition">The position in the stack at the location of the thing to describe is.</param>
        /// <param name="playerToDescribeFor">The player to describe the thing for.</param>
        public LookAtOperation(ushort thingId, Location location, byte stackPosition, IPlayer playerToDescribeFor)
            : base(playerToDescribeFor?.Id ?? 0)
        {
            this.ThingId = thingId;
            this.Location = location;
            this.StackPosition = stackPosition;
            this.PlayerToDescribeFor = playerToDescribeFor;
        }

        /// <summary>
        /// Gets the id of the thing to describe.
        /// </summary>
        public ushort ThingId { get; }

        /// <summary>
        /// Gets the location where the thing to describe is.
        /// </summary>
        public Location Location { get; }

        /// <summary>
        /// Gets the position in the stack at the location of the thing to describe is.
        /// </summary>
        public byte StackPosition { get; }

        /// <summary>
        /// Gets the player to describe for.
        /// </summary>
        public IPlayer PlayerToDescribeFor { get; }

        /// <summary>
        /// Executes the operation's logic.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        protected override void Execute(IOperationContext context)
        {
            IThing thing = null;

            if (this.Location.Type != LocationType.Map || this.PlayerToDescribeFor.CanSee(this.Location))
            {
                IContainerItem container;

                // Get thing at location
                switch (this.Location.Type)
                {
                    case LocationType.Map:
                        thing = context.Map.HasTileAt(this.Location, out ITile targetTile) ? targetTile.TopThing : null;
                        break;
                    case LocationType.InsideContainer:
                        container = context.ContainerManager.FindForCreature(this.PlayerToDescribeFor.Id, this.Location.ContainerId);

                        thing = container?[this.Location.ContainerIndex];
                        break;
                    case LocationType.InventorySlot:
                        container = this.PlayerToDescribeFor[(byte)this.Location.Slot] as IContainerItem;

                        thing = container?.Content.FirstOrDefault();
                        break;
                }
            }

            if (thing == null)
            {
                return;
            }

            if (thing != null)
            {
                context.Logger.LogDebug($"Player {this.PlayerToDescribeFor.Name} looking at {thing}. {this.Location} sector: {this.Location.X / 32}-{this.Location.Y / 32}-{this.Location.Z:00}");
            }

            string description = string.Empty;

            if (thing is IItem itemToDescribe)
            {
                description = this.DescribeItem(itemToDescribe);
            }
            else if (thing is ICreature creatureToDescribe)
            {
                description = $"{creatureToDescribe.Article} {creatureToDescribe.Name}.";
            }

            // TODO: support other things.
            if (string.IsNullOrWhiteSpace(description))
            {
                return;
            }

            description = $"You see {description}";

            this.SendNotification(
                context,
                new TextMessageNotification(this.PlayerToDescribeFor.YieldSingleItem(), MessageType.CenterGreen, description));
        }

        private string DescribeItem(IItem itemToDescribe)
        {
            string description = $"{itemToDescribe.Type.Name}";

            if (itemToDescribe.Type.HasItemFlag(ItemFlag.IsLiquidPool) && itemToDescribe.Attributes.ContainsKey(ItemAttribute.LiquidType))
            {
                var liquidTypeName = ((LiquidType)itemToDescribe.Attributes[ItemAttribute.LiquidType]).ToString().ToLower();

                description += $" of {liquidTypeName}";
            }

            if (itemToDescribe.Type.HasItemFlag(ItemFlag.IsLiquidContainer) && itemToDescribe.Attributes.ContainsKey(ItemAttribute.LiquidType))
            {
                var liquidTypeName = ((LiquidType)itemToDescribe.Attributes[ItemAttribute.LiquidType]).ToString().ToLower();

                description += $" of {liquidTypeName}";
            }

            description += $".";

            if (itemToDescribe.Amount > 1)
            {
                // TODO: naive solution, add S to pluralize will produce some spelling errors.
                description = $"{itemToDescribe.Amount} {description.TrimStartArticles().TrimEnd('.')}s.";
            }

            if (!string.IsNullOrWhiteSpace(itemToDescribe.Type.Description))
            {
                description += $"\n{itemToDescribe.Type.Description}";
            }

            if (itemToDescribe.Type.HasItemFlag(ItemFlag.IsReadable) && itemToDescribe.Attributes.ContainsKey(ItemAttribute.Text) && itemToDescribe.Attributes.ContainsKey(ItemAttribute.ReadRange))
            {
                var text = itemToDescribe.Attributes[ItemAttribute.Text] as string;
                var fontSize = itemToDescribe.Attributes[ItemAttribute.ReadRange] as int? ?? 0;

                var locationDiff = itemToDescribe.Location - this.PlayerToDescribeFor.Location;
                var readingDistance = itemToDescribe.CarryLocation != null ? 0 : itemToDescribe.Location.Type != LocationType.Map ? 0 : locationDiff.MaxValueIn2D + Math.Abs(locationDiff.Z * 10);

                switch (fontSize)
                {
                    case 0:
                        description += readingDistance <= 1 ? $"\n{Regex.Unescape(text).Trim('"')}." : string.Empty;
                        break;
                    case 1:
                        // Only on use, so nothing to add here.
                        break;
                    default:
                        // Distance calculation.
                        description += readingDistance <= fontSize ? $" It reads:\n{Regex.Unescape(text).Trim('"')}" : " You are too far away to read it.";
                        break;
                }
            }

            return Regex.Replace(description, @"[^\u0000-\u007F]+", string.Empty);
        }
    }
}
