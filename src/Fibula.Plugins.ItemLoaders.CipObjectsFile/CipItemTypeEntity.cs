// -----------------------------------------------------------------
// <copyright file="CipItemTypeEntity.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Plugins.ItemLoaders.CipObjectsFile
{
    using System;
    using System.Collections.Generic;
    using Fibula.Data.Entities.Contracts.Abstractions;
    using Fibula.Definitions.Enumerations;
    using Fibula.Definitions.Flags;

    /// <summary>
    /// Class that represents an item type entity derived from the Cip objects file.
    /// </summary>
    public sealed class CipItemTypeEntity : IItemTypeEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CipItemTypeEntity"/> class.
        /// </summary>
        public CipItemTypeEntity()
        {
            this.TypeId = 0;
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.DefaultAttributes = new Dictionary<byte, IConvertible>();
        }

        /// <summary>
        /// Gets or sets the id of the type of this item.
        /// </summary>
        public ushort TypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of this type of item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the decription of this type of item.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the flags for this type of item.
        /// </summary>
        public ulong Flags { get; set; }

        /// <summary>
        /// Gets the attributes of this type of item.
        /// </summary>
        // TODO: get rid of this and add all attributes as properties?
        public IDictionary<byte, IConvertible> DefaultAttributes { get; }

        /// <summary>
        /// Gets or sets the client id of the type of this item.
        /// </summary>
        public ushort ClientId { get; set; }

        /// <summary>
        /// Gets or sets the id of this type.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Clones the <see cref="CipItemTypeEntity"/> into a new instance without locking the clone.
        /// </summary>
        /// <returns>The cloned <see cref="CipItemTypeEntity"/>.</returns>
        object ICloneable.Clone()
        {
            var newInstance = new CipItemTypeEntity()
            {
                TypeId = this.TypeId,
                Name = this.Name,
                Description = this.Description,
            };

            newInstance.Flags = this.Flags;

            foreach (var attribute in this.DefaultAttributes)
            {
                newInstance.DefaultAttributes.Add(attribute);
            }

            return newInstance;
        }

        /// <summary>
        /// Sets a flag in this type.
        /// </summary>
        /// <param name="itemFlag">The flag to set in the type.</param>
        public void SetItemFlag(ItemFlag itemFlag)
        {
            this.Flags |= (ulong)itemFlag;
        }

        /// <summary>
        /// Sets an attribute in this type.
        /// </summary>
        /// <param name="attribute">The attribute to set in the type.</param>
        /// <param name="attributeValue">The value of the attribute to set in the type.</param>
        public void SetAttribute(ItemAttribute attribute, int attributeValue)
        {
            this.DefaultAttributes[(byte)attribute] = attributeValue;
        }
    }
}
