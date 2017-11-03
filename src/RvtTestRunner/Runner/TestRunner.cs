// <copyright file="TestRunner.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using Xunit;

    public class TestRunner
    {
        [NotNull]
        private readonly IRunnerLogger _logger;

        public TestRunner([NotNull] IRunnerLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public int Run([NotNull] List<(string AssemblyFileName, string ConfigFile)> assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            using (AssemblyHelper.SubscribeResolve())
            {
                var runner = new RvtRunner(_logger);

                /*
                var reporters = GetAvailableRunnerReporters();
                var result = _reporter;
                if (!NoAutoReporters)
                {
                    result = reporters.FirstOrDefault(r => r.IsEnvironmentallyEnabled) ?? result;
                }

                var reporter = result ?? new DefaultRunnerReporterWithTypes();*/

                var reporter = new DefaultRunnerReporterWithTypes();

                var options = new TestRunOptions(assemblies, reporter);

                try
                {
                    var result = runner.EntryPoint(options);
                    return result;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"error: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    throw;
                }
                catch (BadImageFormatException ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        /*
        private static List<IRunnerReporter> GetAvailableRunnerReporters()
        {
            var result = new List<IRunnerReporter>();

            ////var runnerPath = Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);
            var runnerPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ThrowIfNull();

            foreach (var dllFile in Directory.GetFiles(runnerPath, "*.dll").Select(f => Path.Combine(runnerPath, f)))
            {
                Type[] types;

                try
                {
                    var assembly = Assembly.LoadFile(dllFile);

                    // Originally for non-NET452
                    ////var assembly = Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(dllFile)));

                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
#pragma warning disable CS0618
                    if (type == null || type.GetTypeInfo().IsAbstract || type == typeof(DefaultRunnerReporter) || type == typeof(DefaultRunnerReporterWithTypes)
                        || type.GetInterfaces().All(t => t != typeof(IRunnerReporter)))
                    {
                        continue;
                    }
#pragma warning restore CS0618
                    var ctor = type.GetConstructor(Array.Empty<Type>());
                    if (ctor == null)
                    {
                        Console.WriteLine($"Type {type.FullName} in assembly {dllFile} appears to be a runner reporter, but does not have an empty constructor.");
                        continue;
                    }

                    result.Add((IRunnerReporter)ctor.Invoke(Array.Empty<object>()));
                }
            }

            return result;
        }*/
    }

    /*
    internal class DependencyContext
    {
        private static readonly Lazy<DependencyContext> _defaultContext = new Lazy<DependencyContext>(LoadDefault);

        public DependencyContext(TargetInfo target,
            CompilationOptions compilationOptions,
            IEnumerable<CompilationLibrary> compileLibraries,
            IEnumerable<RuntimeLibrary> runtimeLibraries,
            IEnumerable<RuntimeFallbacks> runtimeGraph)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            if (compilationOptions == null)
            {
                throw new ArgumentNullException(nameof(compilationOptions));
            }
            if (compileLibraries == null)
            {
                throw new ArgumentNullException(nameof(compileLibraries));
            }
            if (runtimeLibraries == null)
            {
                throw new ArgumentNullException(nameof(runtimeLibraries));
            }
            if (runtimeGraph == null)
            {
                throw new ArgumentNullException(nameof(runtimeGraph));
            }

            Target = target;
            CompilationOptions = compilationOptions;
            CompileLibraries = compileLibraries.ToArray();
            RuntimeLibraries = runtimeLibraries.ToArray();
            RuntimeGraph = runtimeGraph.ToArray();
        }

        public static DependencyContext Default => _defaultContext.Value;

        public TargetInfo Target { get; }

        public CompilationOptions CompilationOptions { get; }

        public IReadOnlyList<CompilationLibrary> CompileLibraries { get; }

        public IReadOnlyList<RuntimeLibrary> RuntimeLibraries { get; }

        public IReadOnlyList<RuntimeFallbacks> RuntimeGraph { get; }

        public DependencyContext Merge(DependencyContext other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return new DependencyContext(
                Target,
                CompilationOptions,
                CompileLibraries.Union(other.CompileLibraries, new LibraryMergeEqualityComparer<CompilationLibrary>()),
                RuntimeLibraries.Union(other.RuntimeLibraries, new LibraryMergeEqualityComparer<RuntimeLibrary>()),
                RuntimeGraph.Union(other.RuntimeGraph)
                );
        }

        private static DependencyContext LoadDefault()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                return null;
            }

            return Load(entryAssembly);
        }

        public static DependencyContext Load(Assembly assembly)
        {
            return DependencyContextLoader.Default.Load(assembly);
        }

        private class LibraryMergeEqualityComparer<T> : IEqualityComparer<T> where T : Library
        {
            public bool Equals(T x, T y)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name);
            }

            public int GetHashCode(T obj)
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
            }
        }
    }*/
}