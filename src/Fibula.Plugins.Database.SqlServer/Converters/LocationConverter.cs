// -----------------------------------------------------------------
// <copyright file="LocationConverter.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Plugins.Database.SqlServer.Converters
{
    using System;
    using System.Linq.Expressions;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Utilities.Validation;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    /// <summary>
    /// Class that represents a converter from <see cref="string"/> to <see cref="Location"/>.
    /// </summary>
    public class LocationConverter : ValueConverter<Location, string>
    {
        private static readonly Expression<Func<string, Location>> ConvertFromProviderExp = (storedStr) => ParseLocation(storedStr);

        private static readonly Expression<Func<Location, string>> ConvertToProviderExp = (location) => location.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationConverter"/> class.
        /// </summary>
        public LocationConverter()
            : base(ConvertToProviderExp, ConvertFromProviderExp, null)
        {
        }

        /// <summary>
        /// Converts a string into a <see cref="Location"/>.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The location converted.</returns>
        public static Location ParseLocation(string value)
        {
            value.ThrowIfNullOrWhiteSpace(nameof(value));

            var coordsArray = value.TrimStart('[').TrimEnd(']').Split(',');

            if (coordsArray.Length != 3)
            {
                throw new ArgumentException($"Invalid location string '{value}'.");
            }

            return new Location
            {
                X = System.Convert.ToInt32(coordsArray[0]),
                Y = System.Convert.ToInt32(coordsArray[1]),
                Z = System.Convert.ToSByte(coordsArray[2]),
            };
        }
    }
}
