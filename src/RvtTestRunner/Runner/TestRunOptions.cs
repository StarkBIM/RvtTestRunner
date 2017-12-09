// <copyright file="TestRunOptions.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    using JetBrains.Annotations;

    using RvtTestRunner.Util;

    using Xunit;

    /// <summary>
    ///     The set of options for a test run
    /// </summary>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1600:ElementsMustBeDocumented",
        Justification = "Temporary - need to review options. Some are console-specific and irrelevant")]
    public class TestRunOptions
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TestRunOptions" /> class.
        /// </summary>
        /// <param name="assemblies">
        ///     A list of tuples containing the assembly path and the assembly configuration file
        ///     (configuration file is not yet supported)
        /// </param>
        /// <param name="reporter">The reporter</param>
        public TestRunOptions([NotNull] List<(string AssemblyFileName, string ConfigFile)> assemblies, [NotNull] IRunnerReporter reporter)
        {
            Reporter = reporter;
            Project = GetProjectFile(assemblies);
        }

        public bool Debug { get; set; }

        public bool DiagnosticMessages { get; set; }

        public bool InternalDiagnosticMessages { get; set; }

        public bool FailSkips { get; set; }

        [CanBeNull]
        public int? MaxParallelThreads { get; set; }

        public bool NoAppDomain { get; set; }

        public bool NoAutoReporters { get; set; }

        public bool NoColor { get; set; }

        public bool NoLogo { get; set; }

#if DEBUG
        public bool Pause { get; set; }
#endif

        [NotNull]
        [ItemNotNull]
        public XunitProject Project { get; }

        [CanBeNull]
        public bool? ParallelizeAssemblies { get; set; }

        [CanBeNull]
        public bool? ParallelizeTestCollections { get; set; }

        [NotNull]
        public IRunnerReporter Reporter { get; }

        public bool Serialize { get; set; }

        public bool StopOnFail { get; set; }

        public bool Wait { get; set; }

        [NotNull]
        private static string GetFullPath([NotNull] string fileName) => Path.GetFullPath(fileName);

        [NotNull]
        [ItemNotNull]
        private static XunitProject GetProjectFile([NotNull] IEnumerable<(string AssemblyFileName, string ConfigFile)> assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            var result = new XunitProject();

            foreach (var (assemblyFileName, configFile) in assemblies)
            {
                result.Add(
                           new XunitProjectAssembly
                           {
                               AssemblyFilename = GetFullPath(assemblyFileName),
                               ConfigFilename = !configFile.IsNullOrWhiteSpace() ? GetFullPath(configFile) : null
                           });
            }

            return result;
        }

        /*
                private static void GuardNoOptionValue(KeyValuePair<string, string> option)
                {
                    if (option.Value != null)
                    {
                        throw new ArgumentException($"error: unknown command line option: {option.Value}");
                    }
                }
        */

        /*
                private static bool IsConfigFile(string fileName)
                {
                    return fileName.EndsWith(".config", StringComparison.OrdinalIgnoreCase)
                           || fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
                }
        */
        /*
        protected XunitProject Parse(Predicate<string> fileExists)
        {
            var assemblies = new List<Tuple<string, string>>();

            while (arguments.Count > 0)
            {
                if (arguments.Peek().StartsWith("-", StringComparison.Ordinal))
                    break;

                var assemblyFile = arguments.Pop();
                if (IsConfigFile(assemblyFile))
                    throw new ArgumentException($"expecting assembly, got config file: {assemblyFile}");
                if (!fileExists(assemblyFile))
                    throw new ArgumentException($"file not found: {assemblyFile}");

                string configFile = null;
                if (arguments.Count > 0)
                {
                    var value = arguments.Peek();
                    if (!value.StartsWith("-", StringComparison.Ordinal) && IsConfigFile(value))
                    {
                        configFile = arguments.Pop();
                        if (!fileExists(configFile))
                            throw new ArgumentException($"config file not found: {configFile}");
                    }
                }

                assemblies.Add(Tuple.Create(assemblyFile, configFile));
            }

            if (assemblies.Count == 0)
                throw new ArgumentException("must specify at least one assembly");

            var project = GetProjectFile(assemblies);

            while (arguments.Count > 0)
            {
                var option = PopOption(arguments);
                var optionName = option.Key.ToLowerInvariant();

                if (!optionName.StartsWith("-", StringComparison.Ordinal))
                    throw new ArgumentException($"unknown command line option: {option.Key}");

                optionName = optionName.Substring(1);

                if (optionName == "nologo")
                {
                    GuardNoOptionValue(option);
                    NoLogo = true;
                }
                else if (optionName == "failskips")
                {
                    GuardNoOptionValue(option);
                    FailSkips = true;
                }
                else if (optionName == "stoponfail")
                {
                    GuardNoOptionValue(option);
                    StopOnFail = true;
                }
                else if (optionName == "nocolor")
                {
                    GuardNoOptionValue(option);
                    NoColor = true;
                }
                else if (optionName == "noappdomain")
                {
                    GuardNoOptionValue(option);
                    NoAppDomain = true;
                }
                else if (optionName == "noautoreporters")
                {
                    GuardNoOptionValue(option);
                    NoAutoReporters = true;
                }
#if DEBUG
                else if (optionName == "pause")
                {
                    GuardNoOptionValue(option);
                    Pause = true;
                }
#endif
                else if (optionName == "debug")
                {
                    GuardNoOptionValue(option);
                    Debug = true;
                }
                else if (optionName == "serialize")
                {
                    GuardNoOptionValue(option);
                    Serialize = true;
                }
                else if (optionName == "wait")
                {
                    GuardNoOptionValue(option);
                    Wait = true;
                }
                else if (optionName == "diagnostics")
                {
                    GuardNoOptionValue(option);
                    DiagnosticMessages = true;
                }
                else if (optionName == "internaldiagnostics")
                {
                    GuardNoOptionValue(option);
                    InternalDiagnosticMessages = true;
                }
                else if (optionName == "maxthreads")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -maxthreads");

                    switch (option.Value)
                    {
                        case "default":
                            MaxParallelThreads = 0;
                            break;

                        case "unlimited":
                            MaxParallelThreads = -1;
                            break;

                        default:
                            int threadValue;
                            if (!int.TryParse(option.Value, out threadValue) || threadValue < 1)
                                throw new ArgumentException("incorrect argument value for -maxthreads (must be 'default', 'unlimited', or a positive number)");

                            MaxParallelThreads = threadValue;
                            break;
                    }
                }
                else if (optionName == "parallel")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -parallel");

                    ParallelismOption parallelismOption;
                    if (!Enum.TryParse(option.Value, out parallelismOption))
                        throw new ArgumentException("incorrect argument value for -parallel");

                    switch (parallelismOption)
                    {
                        case ParallelismOption.all:
                            ParallelizeAssemblies = true;
                            ParallelizeTestCollections = true;
                            break;

                        case ParallelismOption.assemblies:
                            ParallelizeAssemblies = true;
                            ParallelizeTestCollections = false;
                            break;

                        case ParallelismOption.collections:
                            ParallelizeAssemblies = false;
                            ParallelizeTestCollections = true;
                            break;

                        default:
                            ParallelizeAssemblies = false;
                            ParallelizeTestCollections = false;
                            break;
                    }
                }
                else if (optionName == "noshadow")
                {
                    GuardNoOptionValue(option);
                    foreach (var assembly in project.Assemblies)
                        assembly.Configuration.ShadowCopy = false;
                }
                else if (optionName == "trait")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -trait");

                    var pieces = option.Value.Split('=');
                    if (pieces.Length != 2 || string.IsNullOrEmpty(pieces[0]) || string.IsNullOrEmpty(pieces[1]))
                        throw new ArgumentException("incorrect argument format for -trait (should be \"name=value\")");

                    var name = pieces[0];
                    var value = pieces[1];
                    project.Filters.IncludedTraits.Add(name, value);
                }
                else if (optionName == "notrait")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -notrait");

                    var pieces = option.Value.Split('=');
                    if (pieces.Length != 2 || string.IsNullOrEmpty(pieces[0]) || string.IsNullOrEmpty(pieces[1]))
                        throw new ArgumentException("incorrect argument format for -notrait (should be \"name=value\")");

                    var name = pieces[0];
                    var value = pieces[1];
                    project.Filters.ExcludedTraits.Add(name, value);
                }
                else if (optionName == "class")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -class");

                    project.Filters.IncludedClasses.Add(option.Value);
                }
                else if (optionName == "method")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -method");

                    project.Filters.IncludedMethods.Add(option.Value);
                }
                else if (optionName == "namespace")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -namespace");

                    project.Filters.IncludedNameSpaces.Add(option.Value);
                }
                else
                {
                    // Might be a reporter...
                    var maybeReporter = reporters.FirstOrDefault(r => string.Equals(r.RunnerSwitch, optionName, StringComparison.OrdinalIgnoreCase));
                    if (maybeReporter != null)
                    {
                        GuardNoOptionValue(option);
                        if (reporter != null)
                            throw new ArgumentException("only one reporter is allowed");

                        reporter = maybeReporter;
                    }
                    // ...or an result output file
                    else
                    {
                        if (!TransformFactory.AvailableTransforms.Any(t => t.CommandLine.Equals(optionName, StringComparison.OrdinalIgnoreCase)))
                            throw new ArgumentException($"unknown option: {option.Key}");

                        if (option.Value == null)
                            throw new ArgumentException($"missing filename for {option.Key}");

                        EnsurePathExists(option.Value);

                        project.Output.Add(optionName, option.Value);
                    }
                }
            }

            return project;
        }

        private static KeyValuePair<string, string> PopOption(Stack<string> arguments)
        {
            var option = arguments.Pop();
            string value = null;

            if (arguments.Count > 0 && !arguments.Peek().StartsWith("-", StringComparison.Ordinal))
                value = arguments.Pop();

            return new KeyValuePair<string, string>(option, value);
        }

        private static void EnsurePathExists(string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(directory))
                return;

            Directory.CreateDirectory(directory);
        }
        */
    }
}