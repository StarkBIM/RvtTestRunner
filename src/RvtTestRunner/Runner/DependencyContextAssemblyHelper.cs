// <copyright file="DependencyContextAssemblyHelper.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.IO;
    using System.Reflection;

    using JetBrains.Annotations;

    using Microsoft.Extensions.DependencyModel;

    using RvtTestRunner.Util;

    using Xunit.Abstractions;
    using Xunit.Sdk;

    /// <summary>
    ///     Assembly resolution helper for the dependency context
    /// </summary>
    internal class DependencyContextAssemblyHelper : IDisposable
    {
        [NotNull]
        private static readonly DependencyContextJsonReader JsonReader = new DependencyContextJsonReader();

        [NotNull]
        private readonly DependencyContextAssemblyCache _assemblyCache;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DependencyContextAssemblyHelper" /> class.
        /// </summary>
        /// <param name="assemblyFolder">The assembly folder</param>
        /// <param name="dependencyContext">The dependency context</param>
        /// <param name="internalDiagnosticsMessageSink">The message ink, optionally</param>
        public DependencyContextAssemblyHelper(
            [NotNull] string assemblyFolder,
            [NotNull] DependencyContext dependencyContext,
            [CanBeNull] IMessageSink internalDiagnosticsMessageSink)
        {
            _assemblyCache = new DependencyContextAssemblyCache(assemblyFolder, dependencyContext, internalDiagnosticsMessageSink);

            AppDomain.CurrentDomain.AssemblyResolve += OnResolving;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DependencyContextAssemblyHelper"/> class.
        /// </summary>
        ~DependencyContextAssemblyHelper()
        {
            Dispose(false);
        }

        [CanBeNull]
        public static IDisposable SubscribeResolveForAssembly([NotNull] string assemblyFileName, [CanBeNull] IMessageSink internalDiagnosticsMessageSink)
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

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= OnResolving;

            if (disposing)
            {
            }
        }

        [CanBeNull]
        private Assembly OnResolving([NotNull] object sender, [NotNull] ResolveEventArgs args)
            => _assemblyCache.LoadManagedDll(new AssemblyName(args.Name).Name, Assembly.LoadFile);
    }
}