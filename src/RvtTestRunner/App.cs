// <copyright file="App.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner
{
    using System.Reflection;

    using Autodesk.Revit.UI;

    using JetBrains.Annotations;

    public class App : IExternalApplication
    {
        /// <inheritdoc />
        public Result OnShutdown([NotNull] UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        /// <inheritdoc />
        public Result OnStartup([NotNull] UIControlledApplication application)
        {
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(Tab.AddIns, "Test Runner");

            var buttonData = new PushButtonData("TestRunner", "Test Runner", Assembly.GetExecutingAssembly().Location, typeof(RunnerCommand).FullName)
                {
                    AvailabilityClassName = typeof(AlwaysAvailable).FullName
                };

            ribbonPanel.AddItem(buttonData);

            return Result.Succeeded;
        }
    }
}