// <copyright file="AssemblyExtensions.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.IO;
    using System.Reflection;
    using JetBrains.Annotations;

    internal static class AssemblyExtensions
    {
        [CanBeNull]
        public static string GetLocalCodeBase([CanBeNull] this Assembly assembly)
        {
            return GetLocalCodeBase(assembly?.CodeBase, Path.DirectorySeparatorChar);
        }

        [CanBeNull]
        public static string GetLocalCodeBase([CanBeNull] string codeBase, char directorySeparator)
        {
            if (codeBase == null)
            {
                return null;
            }

            if (!codeBase.StartsWith("file://", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Codebase '{codeBase}' is unsupported; must start with 'file://'.", nameof(codeBase));
            }

            // "file:///path" is a local path; "file://machine/path" is a UNC
            var localFile = codeBase.Length > 7 && codeBase[7] == '/';

            // POSIX-style directories
            if (directorySeparator == '/')
            {
                if (localFile)
                {
                    return codeBase.Substring(7);
                }

                throw new ArgumentException($"UNC-style codebase '{codeBase}' is not supported on POSIX-style file systems.", nameof(codeBase));
            }

            // Windows-style directories
            if (directorySeparator != '\\')
            {
                throw new ArgumentException($"Unknown directory separator '{directorySeparator}'; must be one of '/' or '\\'.", nameof(directorySeparator));
            }

            codeBase = codeBase.Replace('/', '\\');

            return codeBase.Substring(localFile ? 8 : 5);
        }
    }
}