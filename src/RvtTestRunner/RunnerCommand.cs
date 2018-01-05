// <copyright file="RunnerCommand.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Windows;
    using System.Windows.Interop;

    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using JetBrains.Annotations;

    using RvtTestRunner.Runner;
    using RvtTestRunner.UI;
    using RvtTestRunner.Util;

    using Xunit;

    /// <summary>
    ///     The external command that will allow a user to select assemblies and run tests
    ///     Next steps:
    ///     Run tests in background (but on Revit thread) while keeping the window open
    ///     Add all of the various options that XUnit provides to the window
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class RunnerCommand : IExternalCommand
    {
        /// <summary>
        ///     Gets a static instance of the external command data used by this class, which is available for unit tests to use
        /// </summary>
        [CanBeNull]
        public static ExternalCommandData CommandData { get; private set; }

        /// <inheritdoc />
        public Result Execute([NotNull] ExternalCommandData commandData, [CanBeNull] ref string message, [CanBeNull] [ItemNotNull] ElementSet elements)
        {
            try
            {
                CommandData = commandData ?? throw new ArgumentNullException(nameof(commandData));

                var testRunnerControlViewModel = new TestRunnerControlViewModel(DispatcherScheduler.Current);

                var mainWindowHandlePtr = Process.GetCurrentProcess().MainWindowHandle;

                var testRunnerWindow = new TestRunnerWindow
                {
                    DataContext = testRunnerControlViewModel,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                // ReSharper disable once UnusedVariable
                var wih = new WindowInteropHelper(testRunnerWindow)
                {
                    Owner = mainWindowHandlePtr
                };

                if (testRunnerWindow.ShowDialog() != true)
                {
                    return Result.Cancelled;
                }

                var selectedAssemblyList = testRunnerControlViewModel.SelectedAssemblies.ToList();

                if (!selectedAssemblyList.Any())
                {
                    TaskDialog.Show("Error", "No assemblies selected");
                    return Result.Failed;
                }

                var nonExistentAssemblies = selectedAssemblyList.Where(a => !File.Exists(a)).ToList();

                if (nonExistentAssemblies.Any())
                {
                    TaskDialog.Show("Error", $"One or more assemblies does not exist: {nonExistentAssemblies.JoinList(", ")}");
                    return Result.Failed;
                }

                List<string> assemblyList = BuildAssemblyList(testRunnerControlViewModel);

                var stopWatch = Stopwatch.StartNew();

                var logger = new RvtRunnerLogger(stopWatch);
                var testRunner = new TestRunner(logger);

                var reporter = new DefaultRunnerReporterWithTypes();

                var options = new TestRunOptions(assemblyList, reporter);

                SetEnableCommandDataOption(options, testRunnerControlViewModel.AllowCommandDataAccess);

                var result = testRunner.Run(options);

                stopWatch.Stop();

                var allMessages = logger.AllMessages.JoinList();

                new TaskDialog("Result")
                {
                    MainInstruction = $"Failing tests: {result}. Total time elapsed: {stopWatch.Elapsed}",
                    ExpandedContent = allMessages
                }.Show();

                Debug.WriteLine(result);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"{ex.GetType()} - {ex.Message}");
                return Result.Failed;
            }
            finally
            {
                CommandData = null;
            }

            return Result.Succeeded;
        }

        [NotNull]
        [ItemNotNull]
        private static List<string> BuildAssemblyList([NotNull] TestRunnerControlViewModel testRunnerControlViewModel)
        {
            var selectedAssemblyList = testRunnerControlViewModel.SelectedAssemblies.ToList();

            if (!testRunnerControlViewModel.CopyDllsToNewFolder)
            {
                return selectedAssemblyList;
            }

            var assemblyList = new List<string>();

            string tempPath = Path.GetTempPath();
            string tempFolder = Path.Combine(tempPath, "StarkBIM");
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            foreach (var assembly in selectedAssemblyList)
            {
                string sourceDirectory = Path.GetDirectoryName(assembly).ThrowIfNull();

                var folderName = $"{DateTime.Now.Ticks}";
                var destinationDirectory = Path.Combine(tempFolder, folderName);
                Directory.CreateDirectory(destinationDirectory);

                // Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourceDirectory, destinationDirectory));
                }

                // Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(sourceDirectory, destinationDirectory), true);
                }

                string newAssembly = assembly.Replace(sourceDirectory, destinationDirectory);
                assemblyList.Add(newAssembly);
            }

            return assemblyList;
        }

        private static void SetEnableCommandDataOption([NotNull] TestRunOptions options, bool enableCommandData)
        {
            options.MaxParallelThreads = enableCommandData ? 1 : (int?)null;

            options.NoAppDomain = enableCommandData;

            options.ParallelizeAssemblies = enableCommandData ? false : (bool?)null;

            options.ParallelizeTestCollections = enableCommandData ? false : (bool?)null;
        }
    }
}