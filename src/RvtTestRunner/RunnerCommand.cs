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

    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using JetBrains.Annotations;

    using RvtTestRunner.Runner;
    using RvtTestRunner.UI;
    using RvtTestRunner.Util;

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

                var testRunnerControlViewModel = new TestRunnerControlViewModel();

                var testRunnerWindow = new TestRunnerWindow
                    {
                        DataContext = testRunnerControlViewModel
                    };

                if (testRunnerWindow.ShowDialog() != true)
                {
                    return Result.Cancelled;
                }

                List<string> selectedAssemblyList = testRunnerControlViewModel.SelectedAssemblies.ToList();

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

                var assemblyList = selectedAssemblyList.Select(selectedAssembly => ((string AssemblyFileName, string Config))(selectedAssembly, null)).ToList();

                Stopwatch stopWatch = Stopwatch.StartNew();

                RvtRunnerLogger logger = new RvtRunnerLogger(stopWatch);
                var testRunner = new TestRunner(logger);

                /*
                // Cheat and hardcode the assembly for now
                const string AssemblyFileName =
                    @"C:\Users\Colin\Source\Repos\SampleRevitAddin\test\StarkBIM.SampleRevitApp.RvtAddin.Test\bin\x64\2017\StarkBIM.SampleRevitApp.RvtAddin.Test.dll";*/

                var result = testRunner.Run(assemblyList);

                stopWatch.Stop();

                string allMessages = logger.AllMessages.JoinList();

                new TaskDialog("Result")
                    {
                        MainInstruction = $"Returned result: {result}. Total time elapsed: {stopWatch.Elapsed}",
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
    }
}