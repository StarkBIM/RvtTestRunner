// <copyright file="XunitPackageCompilationAssemblyResolver.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using JetBrains.Annotations;
    using Microsoft.DotNet.PlatformAbstractions;
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.DependencyModel.Resolution;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "xUnit class")]
    internal class XunitPackageCompilationAssemblyResolver : ICompilationAssemblyResolver
    {
        [NotNull]
        private static readonly IFileSystem FileSystem = new FileSystemWrapper();
        [NotNull]
        [ItemNotNull]
        private readonly List<string> _nugetPackageDirectories;

        public XunitPackageCompilationAssemblyResolver([CanBeNull] IMessageSink internalDiagnosticsMessageSink)
        {
            _nugetPackageDirectories = GetDefaultProbeDirectories(internalDiagnosticsMessageSink);
        }

        public bool TryResolveAssemblyPaths([NotNull] CompilationLibrary library, [NotNull][ItemNotNull] List<string> assemblies)
        {
            if (_nugetPackageDirectories.Count == 0 || !string.Equals(library.Type, "package", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            foreach (var directory in _nugetPackageDirectories)
            {
                if (!ResolverUtils.TryResolvePackagePath(FileSystem, library, directory, out var packagePath))
                {
                    continue;
                }

                if (!TryResolveFromPackagePath(library, packagePath, out var fullPathsFromPackage))
                {
                    continue;
                }

                assemblies.AddRange(fullPathsFromPackage);
                return true;
            }

            return false;
        }

        [NotNull]
        [ItemNotNull]
        private static List<string> GetDefaultProbeDirectories([CanBeNull] IMessageSink internalDiagnosticsMessageSink) =>
            GetDefaultProbeDirectories(RuntimeEnvironment.OperatingSystemPlatform, internalDiagnosticsMessageSink);

        [NotNull]
        [ItemNotNull]
        private static List<string> GetDefaultProbeDirectories(Platform osPlatform, [CanBeNull] IMessageSink internalDiagnosticsMessageSink)
        {
            var results = new HashSet<string>();

#if NETCOREAPP1_0 // The fact that the original code would only use PROBING_DIRECTORIES was causing failures to load
// referenced packages, so instead we'll use PROBING_DIRECTORIES as a supplemental folder.
            var probeDirectories = AppContext.GetData("PROBING_DIRECTORIES");
            var listOfDirectories = probeDirectories as string;

            if (!string.IsNullOrEmpty(listOfDirectories))
                foreach (var directory in listOfDirectories.Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries))
                    results.Add(directory);
#endif

            // Allow the user to override the default location of NuGet packages
            var packageDirectory = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
            if (!string.IsNullOrEmpty(packageDirectory))
            {
                results.Add(packageDirectory);
            }
            else
            {
                string basePath = Environment.GetEnvironmentVariable(osPlatform == Platform.Windows ? "USERPROFILE" : "HOME");

                if (!string.IsNullOrEmpty(basePath))
                {
                    results.Add(Path.Combine(basePath, ".nuget", "packages"));
                }
            }

            internalDiagnosticsMessageSink?.OnMessage(
                                                      new DiagnosticMessage(
                                                                            $"[XunitPackageCompilationAssemblyResolver.GetDefaultProbeDirectories] returns: [{string.Join(",", results.Select(x => $"'{x}'"))}]"));

            return results.ToList();
        }

        [ContractAnnotation("=>true,results:notnull;=>false,results:null")]
        private static bool TryResolveFromPackagePath([NotNull] CompilationLibrary library, [NotNull] string basePath, [CanBeNull][ItemNotNull] out IEnumerable<string> results)
        {
            var paths = new List<string>();

            foreach (var assembly in library.Assemblies)
            {
                if (!ResolverUtils.TryResolveAssemblyFile(FileSystem, basePath, assembly, out var fullName))
                {
                    // if one of the files can't be found, skip this package path completely.
                    // there are package paths that don't include all of the "ref" assemblies
                    // (ex. ones created by 'dotnet store')
                    results = null;
                    return false;
                }

                paths.Add(fullName);
            }

            results = paths;
            return true;
        }
    }
}