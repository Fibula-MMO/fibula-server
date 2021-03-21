// -----------------------------------------------------------------
// <copyright file="BoolConverter.cs" company="2Dudes">
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
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    /// <summary>
    /// Class that represents a converter from <see cref="byte"/> to <see cref="bool"/>.
    /// </summary>
    public class BoolConverter : ValueConverter<bool, byte>
    {
        private static readonly Expression<Func<byte, bool>> ConvertFromProviderExp = (storedInt) => storedInt > 0;

        private static readonly Expression<Func<bool, byte>> ConvertToProviderExp = (boolean) => (byte)(boolean ? 0x01 : 0x00);

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolConverter"/> class.
        /// </summary>
        public BoolConverter()
            : base(ConvertToProviderExp, ConvertFromProviderExp, null)
        {
        }
    }
}
