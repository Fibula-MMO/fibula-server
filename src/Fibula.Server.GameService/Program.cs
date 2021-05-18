// -----------------------------------------------------------------
// <copyright file="Program.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.GameService
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Class that contains the main entry point of the program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point of the service.
        /// </summary>
        /// <param name="args">Any arguments passed down to the program on invocation.</param>
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build();

            host.Run();
        }
    }
}
