// <copyright file="ResolverUtils.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System.IO;

    using Microsoft.Extensions.DependencyModel;

    internal static class ResolverUtils
    {
        internal static bool TryResolvePackagePath(IFileSystem fileSystem, CompilationLibrary library, string basePath, out string packagePath)
        {
            var path = library.Path;
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(library.Name.ToLowerInvariant(), library.Version.ToLowerInvariant());
            }

            packagePath = Path.Combine(basePath, path);

            if (fileSystem.Directory.Exists(packagePath))
            {
                return true;
            }
            return false;
        }

        internal static bool TryResolveAssemblyFile(IFileSystem fileSystem, string basePath, string assemblyPath, out string fullName)
        {
            fullName = Path.Combine(basePath, assemblyPath);
            if (fileSystem.File.Exists(fullName))
            {
                return true;
            }
            return false;
        }
    }
}