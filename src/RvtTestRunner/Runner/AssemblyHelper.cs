// <copyright file="AssemblyHelper.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.IO;
    using System.Reflection;

    using JetBrains.Annotations;

    using RvtTestRunner.Util;

    using Xunit.Abstractions;

    /// <summary>
    ///     This class provides assistance with assembly resolution for missing assemblies. Runners may
    ///     need to use <see cref="SubscribeResolve(string)" /> to help automatically resolve missing assemblies
    ///     when running tests.
    /// </summary>
    public class AssemblyHelper : LongLivedMarshalByRefObject, IDisposable
    {
        [NotNull]
        private readonly string _directory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyHelper" /> class.
        ///     Constructs an instance using the given <paramref name="directory" /> for resolution.
        /// </summary>
        /// <param name="directory">The directory to use for resolving assemblies.</param>
        public AssemblyHelper([NotNull] string directory)
        {
            _directory = directory;

            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AssemblyHelper"/> class.
        /// </summary>
        ~AssemblyHelper()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Subscribes to the current <see cref="AppDomain" /> <see cref="AppDomain.AssemblyResolve" /> event, to
        ///     provide automatic assembly resolution for assemblies in the runner.
        /// </summary>
        /// <param name="path">
        ///     The path from which to resolve the assembly. If null, will resolve to the path of the assembly
        ///     containing this type
        /// </param>
        /// <returns>An object which, when disposed, un-subscribes.</returns>
        [NotNull]
        public static IDisposable SubscribeResolve([CanBeNull] string path = null)
            => new AssemblyHelper(path ?? Path.GetDirectoryName(typeof(AssemblyHelper).Assembly.GetLocalCodeBase()).ThrowIfNull());

        /// <summary>
        ///     Subscribes to the current <see cref="AppDomain" /> <see cref="AppDomain.AssemblyResolve" /> event, to
        ///     provide automatic assembly resolution from an assembly which has a .deps.json file from the .NET SDK
        ///     build process.
        /// </summary>
        /// <param name="assemblyFileName">The assembly file name</param>
        /// <param name="internalDiagnosticsMessageSink">The message sink</param>
        /// <returns>An object which, when disposed, unsubscribes.</returns>
        [CanBeNull]
        public static IDisposable SubscribeResolveForAssembly([NotNull] string assemblyFileName, [CanBeNull] IMessageSink internalDiagnosticsMessageSink = null)
            => DependencyContextAssemblyHelper.SubscribeResolveForAssembly(assemblyFileName, internalDiagnosticsMessageSink);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= Resolve;

            if (disposing)
            {
            }
        }

        [CanBeNull]
        private static Assembly LoadAssembly([NotNull] string assemblyPath)
        {
            try
            {
                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        [CanBeNull]
        private Assembly LoadAssembly([NotNull] AssemblyName assemblyName)
        {
            var path = Path.Combine(_directory, assemblyName.Name);
            return LoadAssembly(path + ".dll") ?? LoadAssembly(path + ".exe");
        }

        [CanBeNull]
        private Assembly Resolve([NotNull] object sender, [NotNull] ResolveEventArgs args)
            => LoadAssembly(new AssemblyName(args.Name));
    }
}