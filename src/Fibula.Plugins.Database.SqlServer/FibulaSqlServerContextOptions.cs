// -----------------------------------------------------------------
// <copyright file="FibulaSqlServerContextOptions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Plugins.Database.SqlServer
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Class that represents options for CosmosDB configuration.
    /// </summary>
    public class FibulaSqlServerContextOptions
    {
        /// <summary>
        /// Gets or sets the secret name of the connection string, used to retrieve the value from the secrets provider.
        /// </summary>
        [Required(ErrorMessage = "A name for the connection string secret is required.")]
        public string ConnectionStringSecretName { get; set; }
    }
}
