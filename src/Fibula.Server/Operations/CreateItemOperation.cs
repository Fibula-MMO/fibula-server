﻿// -----------------------------------------------------------------
// <copyright file="CreateItemOperation.cs" company="2Dudes">
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
    using System.Collections.Generic;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Extensions;
    using Fibula.Server.Contracts.Models;
    using Fibula.Server.Extensions;

    /// <summary>
    /// Class that represents an event for an item creation.
    /// </summary>
    public class CreateItemOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateItemOperation"/> class.
        /// </summary>
        /// <param name="requestorId">The id of the creature requesting the use.</param>
        /// <param name="itemTypeId">The type id of the item being created.</param>
        /// <param name="atLocation">The location from which the item is being created.</param>
        /// <param name="attributes">The attributes to set in the item.</param>
        public CreateItemOperation(uint requestorId, ushort itemTypeId, Location atLocation, IReadOnlyCollection<(ItemAttribute, IConvertible)> attributes)
            : base(requestorId)
        {
            this.TypeId = itemTypeId;
            this.WithAttributes = attributes;
            this.AtLocation = atLocation;
        }

        /// <summary>
        /// Gets the type id of the item to create.
        /// </summary>
        public ushort TypeId { get; }

        /// <summary>
        /// Gets or sets the attributes to set in the item to create.
        /// </summary>
        public IReadOnlyCollection<(ItemAttribute, IConvertible)> WithAttributes { get; set; }

        /// <summary>
        /// Gets the location at which to create the item.
        /// </summary>
        public Location AtLocation { get; }

        /// <summary>
        /// Executes the operation's logic.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        protected override void Execute(IOperationContext context)
        {
            var requestor = this.GetRequestor(context.CreatureManager);
            var inThingContainer = this.AtLocation.DecodeContainer(context.Map, context.ContainerManager, out byte index, requestor);

            var creationArguments = new ItemCreationArguments()
            {
                TypeId = this.TypeId,
                Attributes = this.WithAttributes,
            };

            if (inThingContainer == null || !(context.ItemFactory.CreateItem(creationArguments) is IItem itemCreated))
            {
                return;
            }

            // At this point, we were able to generate the new one, let's proceed to add it.
            inThingContainer.AddItemToContainerRecursively(context.ItemFactory, ref itemCreated, index, includeTileAsFallback: true);
        }
    }
}
