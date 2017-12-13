// <copyright file="XunitExtensionMethods.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.IO;

    using JetBrains.Annotations;

    using Xunit;

    /// <summary>
    /// Extension methods for XUnit classes
    /// </summary>
    public static class XunitExtensionMethods
    {
        /// <summary>
        /// Gets the filename of the assembly for the given xunit project assembly
        /// </summary>
        /// <param name="assembly">The xunit project assembly</param>
        /// <returns>The filename of the assembly file</returns>
        [CanBeNull]
        public static string GetFileName([NotNull] this XunitProjectAssembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return Path.GetFileName(assembly.AssemblyFilename);
        }

        /// <summary>
        /// Gets the filename with no extension of the assembly for the given xunit project assembly
        /// </summary>
        /// <param name="assembly">The xunit project assembly</param>
        /// <returns>The filename with no extension of the assembly file</returns>
        [CanBeNull]
        public static string GetFileNameWithoutExtension([NotNull] this XunitProjectAssembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return Path.GetFileNameWithoutExtension(assembly.AssemblyFilename);
        }
    }
}