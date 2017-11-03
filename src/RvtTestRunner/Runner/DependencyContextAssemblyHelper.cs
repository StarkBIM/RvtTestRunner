// <copyright file="DependencyContextAssemblyHelper.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.IO;
    using System.Reflection;

    using Microsoft.Extensions.DependencyModel;

    using Xunit.Abstractions;
    using Xunit.Sdk;

    internal class DependencyContextAssemblyHelper : IDisposable
    {
        private static readonly DependencyContextJsonReader JsonReader = new DependencyContextJsonReader();

        private readonly DependencyContextAssemblyCache _assemblyCache;

        public DependencyContextAssemblyHelper(string assemblyFolder, DependencyContext dependencyContext, IMessageSink internalDiagnosticsMessageSink)
        {
            _assemblyCache = new DependencyContextAssemblyCache(assemblyFolder, dependencyContext, internalDiagnosticsMessageSink);

            AppDomain.CurrentDomain.AssemblyResolve += OnResolving;
        }

        public static IDisposable SubscribeResolveForAssembly(string assemblyFileName, IMessageSink internalDiagnosticsMessageSink)
        {
            var assemblyFolder = Path.GetDirectoryName(assemblyFileName).ThrowIfNull();
            var depsJsonFile = Path.Combine(assemblyFolder, Path.GetFileNameWithoutExtension(assemblyFileName) + ".deps.json");
            if (!File.Exists(depsJsonFile))
            {
                internalDiagnosticsMessageSink?.OnMessage(
                                                          new DiagnosticMessage(
                                                                                $"[DependencyContextAssemblyHelper.SubscribeResolveForAssembly] Skipping resolution for '{depsJsonFile}': File not found"));

                return null;
            }

            using (var stream = File.OpenRead(depsJsonFile))
            {
                var context = JsonReader.Read(stream);
                if (context != null)
                {
                    return new DependencyContextAssemblyHelper(assemblyFolder, context, internalDiagnosticsMessageSink);
                }

                internalDiagnosticsMessageSink?.OnMessage(
                                                          new DiagnosticMessage(
                                                                                $"[DependencyContextAssemblyHelper.SubscribeResolveForAssembly] Skipping resolution for '{depsJsonFile}': File appears to be malformed"));

                return null;
            }
        }

        public void Dispose()
            => AppDomain.CurrentDomain.AssemblyResolve -= OnResolving;

        private Assembly OnResolving(object sender, ResolveEventArgs args)
            => _assemblyCache.LoadManagedDll(new AssemblyName(args.Name).Name, Assembly.LoadFile);
    }
}