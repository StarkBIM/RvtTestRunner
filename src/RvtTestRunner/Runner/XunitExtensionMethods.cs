﻿// <copyright file="XunitExtensionMethods.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    using JetBrains.Annotations;

    using Xunit;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "xUnit class")]
    public static class XunitExtensionMethods
    {
        [CanBeNull]
        public static string GetFileName([NotNull] this XunitProjectAssembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return Path.GetFileName(assembly.AssemblyFilename);
        }

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