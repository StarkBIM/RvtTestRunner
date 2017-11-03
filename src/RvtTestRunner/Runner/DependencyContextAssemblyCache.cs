// <copyright file="DependencyContextAssemblyCache.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.DotNet.PlatformAbstractions;
    using Microsoft.Extensions.DependencyModel;

    using Xunit.Abstractions;
    using Xunit.Sdk;

    internal class DependencyContextAssemblyCache
    {
        private static readonly Tuple<string, Assembly> ManagedAssemblyNotFound = new Tuple<string, Assembly>(null, null);

        private readonly string _assemblyFolder;

        private readonly XunitPackageCompilationAssemblyResolver _assemblyResolver;

        private readonly IMessageSink _internalDiagnosticsMessageSink;

        private readonly Dictionary<string, Assembly> _managedAssemblyCache;

        private readonly Dictionary<string, Tuple<RuntimeLibrary, RuntimeAssetGroup>> _managedAssemblyMap;

        public DependencyContextAssemblyCache(string assemblyFolder,
                                              DependencyContext dependencyContext,
                                              IMessageSink internalDiagnosticsMessageSink)
        {
            _assemblyFolder = assemblyFolder;
            _internalDiagnosticsMessageSink = internalDiagnosticsMessageSink;

            _assemblyResolver = new XunitPackageCompilationAssemblyResolver(internalDiagnosticsMessageSink);

            internalDiagnosticsMessageSink?.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache..ctor] Runtime graph: [{string.Join(",", dependencyContext.RuntimeGraph.Select(x => $"'{x.Runtime}'"))}]"));

            var currentRuntime = RuntimeEnvironment.GetRuntimeIdentifier();
            var fallbacks = dependencyContext.RuntimeGraph.FirstOrDefault(x => string.Equals(x.Runtime, currentRuntime, StringComparison.OrdinalIgnoreCase));
            HashSet<string> compatibleRuntimes = fallbacks != null ? new HashSet<string>(fallbacks.Fallbacks, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            compatibleRuntimes.Add(currentRuntime);
            compatibleRuntimes.Add(string.Empty);

            internalDiagnosticsMessageSink?.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache..ctor] Compatible runtimes: [{string.Join(",", compatibleRuntimes.Select(x => $"'{x}'"))}]"));

            _managedAssemblyCache = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
            _managedAssemblyMap =
                dependencyContext.RuntimeLibraries
                    .Where(lib => lib.RuntimeAssemblyGroups?.Count > 0)
                    .Select(lib => Tuple.Create(lib, lib.RuntimeAssemblyGroups.FirstOrDefault(libGroup => compatibleRuntimes.Contains(libGroup.Runtime))))
                    .Where(tuple => tuple.Item2?.AssetPaths != null)
                    .SelectMany(tuple => tuple.Item2.AssetPaths.Where(x => x != null).Select(path => Tuple.Create(Path.GetFileNameWithoutExtension(path), Tuple.Create(tuple.Item1, tuple.Item2))))
                    .ToDictionaryIgnoringDuplicateKeys(tuple => tuple.Item1, tuple => tuple.Item2, StringComparer.OrdinalIgnoreCase);

            internalDiagnosticsMessageSink?.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache..ctor] Managed assembly map includes: {string.Join(",", _managedAssemblyMap.Keys.Select(k => $"'{k}'").OrderBy(k => k, StringComparer.OrdinalIgnoreCase))}"));

#if NETCOREAPP1_0
            unmanagedAssemblyCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            unmanagedAssemblyMap =
                dependencyContext.RuntimeLibraries
                                 .Where(lib => lib.NativeLibraryGroups?.Count > 0)
                                 .Select(lib => Tuple.Create(lib, lib.NativeLibraryGroups.FirstOrDefault(libGroup => compatibleRuntimes.Contains(libGroup.Runtime))))
                                 .Where(tuple => tuple.Item2?.AssetPaths != null)
                                 .SelectMany(tuple => tuple.Item2.AssetPaths.Where(x => x != null).Select(path => Tuple.Create(Path.GetFileName(path), Tuple.Create(tuple.Item1, tuple.Item2))))
                                 .ToDictionaryIgnoringDuplicateKeys(tuple => tuple.Item1, tuple => tuple.Item2, StringComparer.OrdinalIgnoreCase);

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache..ctor] Unmanaged assembly map includes: {string.Join(",", unmanagedAssemblyMap.Keys.Select(k => $"'{k}'").OrderBy(k => k, StringComparer.OrdinalIgnoreCase))}"));
#endif
        }

        public Assembly LoadManagedDll(string assemblyName, Func<string, Assembly> managedAssemblyLoader)
        {
            if (_managedAssemblyCache.TryGetValue(assemblyName, out var result))
            {
                return result;
            }

            _internalDiagnosticsMessageSink?.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.LoadManagedDll] Resolving '{assemblyName}'"));

            var tupleResult = ResolveManagedAssembly(assemblyName, managedAssemblyLoader);
            var resolvedAssemblyPath = tupleResult.Item1;
            result = tupleResult.Item2;
            _managedAssemblyCache[assemblyName] = result;

            if (_internalDiagnosticsMessageSink == null)
            {
                return result;
            }

            _internalDiagnosticsMessageSink.OnMessage(
                                                     result == null
                                                         ? new DiagnosticMessage("[DependencyContextAssemblyCache.LoadManagedDll] Resolution failed, passed down to next resolver")
                                                         : new DiagnosticMessage($"[DependencyContextAssemblyCache.LoadManagedDll] Successful: '{resolvedAssemblyPath}'"));

            return result;
        }

        private Tuple<string, Assembly> ResolveManagedAssembly(string assemblyName, Func<string, Assembly> managedAssemblyLoader)
        {
            // Try to find dependency in the local folder
            var assemblyPath = Path.Combine(_assemblyFolder, assemblyName);

            foreach (var extension in new[] { ".dll", ".exe" })
            {
                try
                {
                    var resolvedAssemblyPath = assemblyPath + extension;
                    if (!File.Exists(resolvedAssemblyPath))
                    {
                        continue;
                    }

                    var assembly = managedAssemblyLoader(resolvedAssemblyPath);
                    if (assembly != null)
                    {
                        return Tuple.Create(resolvedAssemblyPath, assembly);
                    }
                }
                catch
                {
                    // ignored
                }
            }

            // Try to find dependency from .deps.json
            if (!_managedAssemblyMap.TryGetValue(assemblyName, out var libraryTuple))
            {
                return ManagedAssemblyNotFound;
            }

            {
                var library = libraryTuple.Item1;
                var assetGroup = libraryTuple.Item2;
                var wrapper = new CompilationLibrary(library.Type, library.Name, library.Version, library.Hash,
                                                     assetGroup.AssetPaths, library.Dependencies, library.Serviceable);

                var assemblies = new List<string>();
                if (_assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies))
                {
                    var resolvedAssemblyPath = assemblies.FirstOrDefault(a => string.Equals(assemblyName, Path.GetFileNameWithoutExtension(a), StringComparison.OrdinalIgnoreCase));
                    if (resolvedAssemblyPath != null)
                    {
                        resolvedAssemblyPath = Path.GetFullPath(resolvedAssemblyPath);

                        var assembly = managedAssemblyLoader(resolvedAssemblyPath);
                        if (assembly != null)
                        {
                            return Tuple.Create(resolvedAssemblyPath, assembly);
                        }

                        _internalDiagnosticsMessageSink?.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.ResolveManagedAssembly] Found assembly path '{resolvedAssemblyPath}' but the assembly would not load"));
                    }
                    else
                    {
                        _internalDiagnosticsMessageSink?.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.ResolveManagedAssembly] Found a resolved path, but could not map a filename in [{string.Join(",", assemblies.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => $"'{k}'"))}]"));
                    }
                }
                else
                {
                    _internalDiagnosticsMessageSink?.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.ResolveManagedAssembly] Found in dependency map, but unable to resolve a path in [{string.Join(",", assetGroup.AssetPaths.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => $"'{k}'"))}]"));
                }
            }

            return ManagedAssemblyNotFound;
        }

#if NETCOREAPP1_0
// Unmanaged DLL support

        static readonly string[] UnmanagedDllFormats = GetUnmanagedDllFormats().ToArray();

        readonly Dictionary<string, string> unmanagedAssemblyCache;
        readonly Dictionary<string, Tuple<RuntimeLibrary, RuntimeAssetGroup>> unmanagedAssemblyMap;

        static IEnumerable<string> GetUnmanagedDllFormats()
        {
            yield return "{0}";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                yield return "{0}.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                yield return "lib{0}.dylib";
                yield return "{0}.dylib";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                yield return "lib{0}.so";
                yield return "{0}.so";
            }
        }

        public IntPtr LoadUnmanagedDll(string unmanagedDllName, Func<string, IntPtr> unmanagedAssemblyLoader)
        {
            if (!unmanagedAssemblyCache.TryGetValue(unmanagedDllName, out var resolvedAssemblyPath))
            {
                if (internalDiagnosticsMessageSink != null)
                    internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.LoadUnmanagedDll] Resolving '{unmanagedDllName}'"));

                resolvedAssemblyPath = ResolveUnmanagedAssembly(unmanagedDllName);
                unmanagedAssemblyCache[unmanagedDllName] = resolvedAssemblyPath;

                if (internalDiagnosticsMessageSink != null)
                {
                    if (resolvedAssemblyPath == null)
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.LoadUnmanagedDll] Resolution failed, passed down to next resolver"));
                    else
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.LoadUnmanagedDll] Successful: '{resolvedAssemblyPath}'"));
                }
            }

            return resolvedAssemblyPath != null ? unmanagedAssemblyLoader(resolvedAssemblyPath) : default;
        }

        public string ResolveUnmanagedAssembly(string unmanagedDllName)
        {
            foreach (var format in UnmanagedDllFormats)
            {
                var formattedUnmanagedDllName = string.Format(format, unmanagedDllName);

                if (unmanagedAssemblyMap.TryGetValue(formattedUnmanagedDllName, out var libraryTuple))
                {
                    var library = libraryTuple.Item1;
                    var assetGroup = libraryTuple.Item2;
                    var wrapper = new CompilationLibrary(library.Type, library.Name, library.Version, library.Hash, assetGroup.AssetPaths, library.Dependencies, library.Serviceable);

                    var assemblies = new List<string>();
                    if (assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies))
                    {
                        var resolvedAssemblyPath = assemblies.FirstOrDefault(a => string.Equals(formattedUnmanagedDllName, Path.GetFileName(a), StringComparison.OrdinalIgnoreCase));
                        if (resolvedAssemblyPath != null)
                            return Path.GetFullPath(resolvedAssemblyPath);

                        if (internalDiagnosticsMessageSink != null)
                            internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.ResolveUnmanagedDll] Found a resolved path, but could not map a filename in [{string.Join(",", assemblies.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => $"'{k}'"))}]"));
                    }
                    else
                    {
                        if (internalDiagnosticsMessageSink != null)
                            internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyCache.ResolveUnmanagedDll] Found in dependency map, but unable to resolve a path in [{string.Join(",", assetGroup.AssetPaths.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => $"'{k}'"))}]"));
                    }
                }
            }

            return null;
        }
#endif
    }
}