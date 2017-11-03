// <copyright file="RunnerCommand.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using JetBrains.Annotations;

    using RvtTestRunner.Runner;

    using Xunit;

    using TaskDialog = Autodesk.Revit.UI.TaskDialog;

    [Transaction(TransactionMode.Manual)]
    public class RunnerCommand : IExternalCommand
    {
        [CanBeNull]
        public static ExternalCommandData CommandData { get; private set; }

        /// <inheritdoc />
        public Result Execute([NotNull] ExternalCommandData commandData, [CanBeNull] ref string message, [CanBeNull] [ItemNotNull] ElementSet elements)
        {
            try
            {
                CommandData = commandData ?? throw new ArgumentNullException(nameof(commandData));

                IRunnerLogger logger = new RvtRunnerLogger();
                var testRunner = new TestRunner(logger);

                // Cheat and hardcode the assembly for now
                const string AssemblyFileName =
                    @"C:\Users\Colin\Source\Repos\SampleRevitAddin\test\StarkBIM.SampleRevitApp.RvtAddin.Test\bin\x64\2017\StarkBIM.SampleRevitApp.RvtAddin.Test.dll";

                if (!File.Exists(AssemblyFileName))
                {
                    TaskDialog.Show("Error", "File does not exist");
                    return Result.Failed;
                }

                (string AssemblyFileName, string Config) assembly =
                    (AssemblyFileName, null);

                var result = testRunner.Run(new List<(string AssemblyFileName, string Config)> { assembly });

                Debug.WriteLine(result);
            }
            finally
            {
                CommandData = null;
            }

            return Result.Succeeded;
        }
    }
}