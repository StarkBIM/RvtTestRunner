// <copyright file="RvtRunner.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using JetBrains.Annotations;

    using RvtTestRunner.Util;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    ///     The test runner that runs inside of Revit
    /// </summary>
    public class RvtRunner
    {
        [NotNull]
        private readonly ConcurrentDictionary<string, ExecutionSummary> _completionMessages = new ConcurrentDictionary<string, ExecutionSummary>();

        [NotNull]
        private readonly IRunnerLogger _logger;

        private volatile bool _cancel;

        private bool _failed;

        [NotNull]
        private IMessageSinkWithTypes _reporterMessageHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RvtRunner" /> class.
        /// </summary>
        /// <param name="logger">The logger</param>
        public RvtRunner([NotNull] IRunnerLogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Runs the tests with the given options
        /// </summary>
        /// <param name="options">The test run options</param>
        /// <returns>The number of failed tests, or -1 if cancelled</returns>
        public int EntryPoint([NotNull] TestRunOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            try
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                var defaultDirectory = Directory.GetCurrentDirectory();
                if (!defaultDirectory.EndsWith(new string(new[] { Path.DirectorySeparatorChar }), StringComparison.Ordinal))
                {
                }

                /*
                if (options.Debug)
                {
                    Debugger.Launch();
                }*/

                _reporterMessageHandler = MessageSinkWithTypesAdapter.Wrap(options.Reporter.CreateMessageHandler(_logger));

                if (!options.NoLogo)
                {
                    PrintHeader();
                }

                var failCount = RunProject(options);

                if (_cancel)
                {
                    return -1;
                }

                return failCount;
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

        [NotNull]
        private static ITestFrameworkExecutionOptions ConfiguExecutionOptions([NotNull] XunitProjectAssembly assembly, [NotNull] TestRunOptions options)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var stopOnFail = options.StopOnFail;

            var executionOptions = TestFrameworkOptions.ForExecution(assembly.Configuration);
            executionOptions.SetStopOnTestFail(stopOnFail);

            var maxThreadCount = options.MaxParallelThreads;

            if (maxThreadCount.HasValue)
            {
                executionOptions.SetMaxParallelThreads(maxThreadCount);
            }

            var parallelizeTestCollections = options.ParallelizeTestCollections;

            if (parallelizeTestCollections.HasValue)
            {
                executionOptions.SetDisableParallelization(!parallelizeTestCollections.GetValueOrDefault());
            }

            return executionOptions;
        }

        private static void ConfigureAssembly([NotNull] XunitProjectAssembly assembly, [NotNull] TestRunOptions options)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var diagnosticMessages = options.DiagnosticMessages;
            var internalDiagnosticMessages = options.InternalDiagnosticMessages;

            // Turn off pre-enumeration of theories, since there is no theory selection UI in this runner
            assembly.Configuration.PreEnumerateTheories = false;
            assembly.Configuration.DiagnosticMessages |= diagnosticMessages;
            assembly.Configuration.InternalDiagnosticMessages |= internalDiagnosticMessages;

            var noAppDomain = options.NoAppDomain;

            if (noAppDomain)
            {
                assembly.Configuration.AppDomain = AppDomainSupport.Denied;
            }
        }

        private static void OnUnhandledException([NotNull] object sender, [NotNull] UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            else
            {
                Console.WriteLine("Error of unknown type thrown in application domain");
            }

            Environment.Exit(1);
        }

        private static void PrintHeader()
        {
            var platform = $"Desktop .NET {Environment.Version}";

            // NET Core
            ////var platform = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

            Console.WriteLine($"xUnit.net Console Runner ({IntPtr.Size * 8}-bit {platform})");
        }

        private static bool ValidateFileExists([CanBeNull] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || File.Exists(fileName))
            {
                return true;
            }

            Console.WriteLine($"File not found: {fileName}");

            return false;
        }

        [CanBeNull]
        private XElement ExecuteAssembly(
            [NotNull] XunitProjectAssembly assembly,
            [NotNull] TestRunOptions options,
            bool needsXml,
            [NotNull] XunitFilters filters)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            if (_cancel)
            {
                return null;
            }

            var internalDiagnosticMessages = options.InternalDiagnosticMessages;

            var assemblyElement = needsXml ? new XElement("assembly") : null;

            try
            {
                if (!ValidateFileExists(assembly.AssemblyFilename) || !ValidateFileExists(assembly.ConfigFilename))
                {
                    return null;
                }

                ConfigureAssembly(assembly, options);

                // Setup discovery and execution options with command-line overrides
                var executionOptions = ConfiguExecutionOptions(assembly, options);

                var assemblyDisplayName = assembly.GetFileNameWithoutExtension().EmptyIfNull();

                void LogAction(MessageHandlerArgs<IDiagnosticMessage> args, string assemblyName) => Console.WriteLine($"{assemblyDisplayName}: {args.Message.Message}");

                var diagnosticMessageSink = new DiagnosticMessageSink(LogAction, assemblyDisplayName, assembly.Configuration.DiagnosticMessagesOrDefault);
                var internalDiagnosticsMessageSink = new DiagnosticMessageSink(LogAction, assemblyDisplayName, assembly.Configuration.InternalDiagnosticMessagesOrDefault);

                var appDomainSupport = assembly.Configuration.AppDomainOrDefault;
                var shadowCopy = assembly.Configuration.ShadowCopyOrDefault;

                using (AssemblyHelper.SubscribeResolveForAssembly(assembly.AssemblyFilename, internalDiagnosticsMessageSink))
                using (var controller = new XunitFrontController(
                                                                 appDomainSupport,
                                                                 assembly.AssemblyFilename,
                                                                 assembly.ConfigFilename,
                                                                 shadowCopy,
                                                                 diagnosticMessageSink: diagnosticMessageSink))
                using (var discoverySink = new TestDiscoverySink(() => _cancel))
                {
                    ExecuteTests(assembly, options, filters, controller, discoverySink, executionOptions, assemblyElement, diagnosticMessageSink);
                }
            }
            catch (Exception ex)
            {
                _failed = true;

                var e = ex;
                while (e != null)
                {
                    Console.WriteLine($"{e.GetType().FullName}: {e.Message}");

                    if (internalDiagnosticMessages)
                    {
                        Console.WriteLine(e.StackTrace);
                    }

                    e = e.InnerException;
                }
            }

            return assemblyElement;
        }

        private void ExecuteTests(
            [NotNull] XunitProjectAssembly assembly,
            [NotNull] TestRunOptions options,
            [NotNull] XunitFilters filters,
            [NotNull] XunitFrontController controller,
            [NotNull] TestDiscoverySink discoverySink,
            [NotNull] ITestFrameworkExecutionOptions executionOptions,
            [CanBeNull] XElement assemblyElement,
            [NotNull] DiagnosticMessageSink diagnosticMessageSink)
        {
            var appDomainSupport = assembly.Configuration.AppDomainOrDefault;
            var shadowCopy = assembly.Configuration.ShadowCopyOrDefault;
            var serialize = options.Serialize;
            var failSkips = options.FailSkips;
            var stopOnFail = options.StopOnFail;

            var longRunningSeconds = assembly.Configuration.LongRunningTestSecondsOrDefault;

            var discoveryOptions = TestFrameworkOptions.ForDiscovery(assembly.Configuration);

            // Discover & filter the tests
            _reporterMessageHandler.OnMessage(
                                              new TestAssemblyDiscoveryStarting(
                                                                                assembly,
                                                                                controller.CanUseAppDomains && appDomainSupport != AppDomainSupport.Denied,
                                                                                shadowCopy,
                                                                                discoveryOptions));

            controller.Find(false, discoverySink, discoveryOptions);
            discoverySink.Finished.WaitOne();

            var testCasesDiscovered = discoverySink.TestCases.Count;
            var filteredTestCases = discoverySink.TestCases.Where(filters.Filter).ToList();
            var testCasesToRun = filteredTestCases.Count;

            _reporterMessageHandler.OnMessage(new TestAssemblyDiscoveryFinished(assembly, discoveryOptions, testCasesDiscovered, testCasesToRun));

            // Run the filtered tests
            if (testCasesToRun == 0)
            {
                _completionMessages.TryAdd(assembly.GetFileName().ThrowIfNull(), new ExecutionSummary());
            }
            else
            {
                if (serialize)
                {
                    filteredTestCases = filteredTestCases.Select(controller.Serialize).Select(controller.Deserialize).ToList();
                }

                _reporterMessageHandler.OnMessage(new TestAssemblyExecutionStarting(assembly, executionOptions));

                IExecutionSink resultsSink = new DelegatingExecutionSummarySink(
                                                                                _reporterMessageHandler,
                                                                                () => _cancel,
                                                                                (path, summary) => _completionMessages.TryAdd(path, summary));
                if (assemblyElement != null)
                {
                    resultsSink = new DelegatingXmlCreationSink(resultsSink, assemblyElement);
                }

                if (longRunningSeconds > 0)
                {
                    resultsSink = new DelegatingLongRunningTestDetectionSink(resultsSink, TimeSpan.FromSeconds(longRunningSeconds), diagnosticMessageSink);
                }

                if (failSkips)
                {
                    resultsSink = new DelegatingFailSkipSink(resultsSink);
                }

                controller.RunTests(filteredTestCases, resultsSink, executionOptions);
                resultsSink.Finished.WaitOne();

                _reporterMessageHandler.OnMessage(new TestAssemblyExecutionFinished(assembly, executionOptions, resultsSink.ExecutionSummary));

                if (!stopOnFail || resultsSink.ExecutionSummary.Failed == 0)
                {
                    return;
                }

                Console.WriteLine("Canceling due to test failure...");
                _cancel = true;
            }
        }

        /*
        private void PrintUsage(IReadOnlyList<IRunnerReporter> reporters)
        {
            var executableName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetLocalCodeBase());

            // NET Core
            ////var executableName = "dotnet xunit";

            Console.WriteLine("Copyright (C) .NET Foundation.");
            Console.WriteLine();
            Console.WriteLine($"usage: {executableName} <assemblyFile> [configFile] [assemblyFile [configFile]...] [options] [reporter] [resultFormat filename [...]]");
            Console.WriteLine();
#if NET452
            Console.WriteLine("Note: Configuration files must end in .json (for JSON) or .config (for XML)");
#else
            Console.WriteLine("Note: Configuration files must end in .json (XML is not supported on .NET Core)");
#endif
            Console.WriteLine();
            Console.WriteLine("Valid options:");
            Console.WriteLine("  -nologo                : do not show the copyright message");
            Console.WriteLine("  -nocolor               : do not output results with colors");
#if NET452
            Console.WriteLine("  -noappdomain           : do not use app domains to run test code");
#endif
            Console.WriteLine("  -failskips             : convert skipped tests into failures");
            Console.WriteLine("  -stoponfail            : stop on first test failure");
            Console.WriteLine("  -parallel option       : set parallelization based on option");
            Console.WriteLine("                         :   none        - turn off all parallelization");
            Console.WriteLine("                         :   collections - only parallelize collections");
            Console.WriteLine("                         :   assemblies  - only parallelize assemblies");
            Console.WriteLine("                         :   all         - parallelize assemblies & collections");
            Console.WriteLine("  -maxthreads count      : maximum thread count for collection parallelization");
            Console.WriteLine("                         :   default   - run with default (1 thread per CPU thread)");
            Console.WriteLine("                         :   unlimited - run with unbounded thread count");
            Console.WriteLine("                         :   (number)  - limit task thread pool size to 'count'");
#if NET452
            Console.WriteLine("  -noshadow              : do not shadow copy assemblies");
#endif
            Console.WriteLine("  -wait                  : wait for input after completion");
            Console.WriteLine("  -diagnostics           : enable diagnostics messages for all test assemblies");
            Console.WriteLine("  -internaldiagnostics   : enable internal diagnostics messages for all test assemblies");
#if DEBUG
            Console.WriteLine("  -pause                 : pause before doing any work, to help attach a debugger");
#endif
            Console.WriteLine("  -debug                 : launch the debugger to debug the tests");
            Console.WriteLine("  -serialize             : serialize all test cases (for diagnostic purposes only)");
            Console.WriteLine("  -trait \"name=value\"    : only run tests with matching name/value traits");
            Console.WriteLine("                         : if specified more than once, acts as an OR operation");
            Console.WriteLine("  -notrait \"name=value\"  : do not run tests with matching name/value traits");
            Console.WriteLine("                         : if specified more than once, acts as an AND operation");
            Console.WriteLine("  -method \"name\"         : run a given test method (can be fully specified or use a wildcard;");
            Console.WriteLine("                         : i.e., 'MyNamespace.MyClass.MyTestMethod' or '*.MyTestMethod')");
            Console.WriteLine("                         : if specified more than once, acts as an OR operation");
            Console.WriteLine("  -class \"name\"          : run all methods in a given test class (should be fully");
            Console.WriteLine("                         : specified; i.e., 'MyNamespace.MyClass')");
            Console.WriteLine("                         : if specified more than once, acts as an OR operation");
            Console.WriteLine("  -namespace \"name\"      : run all methods in a given namespace (i.e.,");
            Console.WriteLine("                         : 'MyNamespace.MySubNamespace')");
            Console.WriteLine("                         : if specified more than once, acts as an OR operation");
            Console.WriteLine("  -noautoreporters       : do not allow reporters to be auto-enabled by environment");
            Console.WriteLine("                         : (for example, auto-detecting TeamCity or AppVeyor)");
#if NETCOREAPP1_0 || NETCOREAPP2_0
            Console.WriteLine("  -framework \"name\"      : set the target framework");
#endif
            Console.WriteLine();

            var switchableReporters = reporters.Where(r => !string.IsNullOrWhiteSpace(r.RunnerSwitch)).ToList();
            if (switchableReporters.Count > 0)
            {
                Console.WriteLine("Reporters: (optional, choose only one)");

                foreach (var reporter in switchableReporters.OrderBy(r => r.RunnerSwitch))
                {
                    Console.WriteLine($"  -{reporter.RunnerSwitch.ToLowerInvariant().PadRight(21)} : {reporter.Description}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Result formats: (optional, choose one or more)");
            TransformFactory.AvailableTransforms.ForEach(
                transform => Console.WriteLine($"  -{$"{transform.CommandLine} <filename>".PadRight(21).Substring(0, 21)} : {transform.Description}")
            );
        }
        */
        private int RunProject([NotNull] TestRunOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var project = options.Project;

            var parallelizeAssemblies = options.ParallelizeAssemblies;

            XElement assembliesElement = null;
            var clockTime = Stopwatch.StartNew();
            var xmlTransformers = TransformFactory.GetXmlTransformers(project);
            var needsXml = xmlTransformers.Count > 0;

            if (!parallelizeAssemblies.HasValue)
            {
                parallelizeAssemblies = project.All(assembly => assembly.Configuration.ParallelizeAssemblyOrDefault);
            }

            if (needsXml)
            {
                assembliesElement = new XElement("assemblies");
            }

            var originalWorkingFolder = Directory.GetCurrentDirectory();

            if (parallelizeAssemblies.GetValueOrDefault())
            {
                var tasks = project.Assemblies.Select(
                                                      assembly => Task.Run(
                                                                           () => ExecuteAssembly(
                                                                                                 assembly,
                                                                                                 options,
                                                                                                 needsXml,
                                                                                                 project.Filters)));
                var results = Task.WhenAll(tasks).GetAwaiter().GetResult();

                if (assembliesElement != null)
                {
                    foreach (var assemblyElement in results.Where(result => result != null))
                    {
                        assembliesElement.Add(assemblyElement);
                    }
                }
            }
            else
            {
                foreach (var assembly in project.Assemblies)
                {
                    var assemblyElement = ExecuteAssembly(
                                                          assembly,
                                                          options,
                                                          needsXml,
                                                          project.Filters);
                    assembliesElement?.Add(assemblyElement);
                }
            }

            clockTime.Stop();

            assembliesElement?.Add(new XAttribute("timestamp", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

            if (_completionMessages.Count > 0)
            {
                _reporterMessageHandler.OnMessage(new TestExecutionSummary(clockTime.Elapsed, _completionMessages.OrderBy(kvp => kvp.Key).ToList()));
            }

            Directory.SetCurrentDirectory(originalWorkingFolder);

            xmlTransformers.ForEach(transformer => transformer(assembliesElement));

            return _failed ? 1 : _completionMessages.Values.Sum(summary => summary.Failed);
        }
    }
}