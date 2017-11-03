// <copyright file="AssemblyHelper.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.IO;
    using System.Reflection;

    using Xunit.Abstractions;

    /// <summary>
    ///     This class provides assistance with assembly resolution for missing assemblies. Runners may
    ///     need to use <see cref="SubscribeResolve(string)" /> to help automatically resolve missing assemblies
    ///     when running tests.
    /// </summary>
    public class AssemblyHelper : LongLivedMarshalByRefObject, IDisposable
    {
        private readonly string _directory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyHelper" /> class.
        ///     Constructs an instance using the given <paramref name="directory" /> for resolution.
        /// </summary>
        /// <param name="directory">The directory to use for resolving assemblies.</param>
        public AssemblyHelper(string directory)
        {
            _directory = directory;

            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        /// <summary>
        ///     Subscribes to the current <see cref="AppDomain" /> <see cref="AppDomain.AssemblyResolve" /> event, to
        ///     provide automatic assembly resolution for assemblies in the runner.
        /// </summary>
        /// <returns>An object which, when disposed, un-subscribes.</returns>
        public static IDisposable SubscribeResolve(string path = null)
            => new AssemblyHelper(path ?? Path.GetDirectoryName(typeof(AssemblyHelper).Assembly.GetLocalCodeBase()));

        /// <summary>
        ///     Subscribes to the current <see cref="AppDomain" /> <see cref="AppDomain.AssemblyResolve" /> event, to
        ///     provide automatic assembly resolution from an assembly which has a .deps.json file from the .NET SDK
        ///     build process.
        /// </summary>
        /// <returns>An object which, when disposed, un-subscribes.</returns>
        public static IDisposable SubscribeResolveForAssembly(string assemblyFileName, IMessageSink internalDiagnosticsMessageSink = null)
            => DependencyContextAssemblyHelper.SubscribeResolveForAssembly(assemblyFileName, internalDiagnosticsMessageSink);

        /// <inheritdoc />
        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= Resolve;
            GC.SuppressFinalize(this);
        }

        private static Assembly LoadAssembly(string assemblyPath)
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

        private Assembly LoadAssembly(AssemblyName assemblyName)
        {
            var path = Path.Combine(_directory, assemblyName.Name);
            return LoadAssembly(path + ".dll") ?? LoadAssembly(path + ".exe");
        }

        private Assembly Resolve(object sender, ResolveEventArgs args)
            => LoadAssembly(new AssemblyName(args.Name));
    }
}