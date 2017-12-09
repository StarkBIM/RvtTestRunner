// <copyright file="App.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner
{
    using System;
    using System.IO;
    using System.Reflection;

    using Autodesk.Revit.UI;

    using JetBrains.Annotations;

    using RvtTestRunner.Util;

    /// <summary>
    ///     The external application class that is loaded by Revit
    /// </summary>
    public class App : IExternalApplication
    {
        [NotNull]
        private string _assemblyDir;

        [CanBeNull]
        private AssemblyResolver _assemblyResolver;

        /// <inheritdoc />
        public Result OnShutdown([NotNull] UIControlledApplication application)
        {
            if (_assemblyResolver != null)
            {
                AppDomain.CurrentDomain.AssemblyResolve -= _assemblyResolver.OnAssemblyResolve;
            }

            return Result.Succeeded;
        }

        /// <inheritdoc />
        public Result OnStartup([NotNull] UIControlledApplication application)
        {
            var location = Assembly.GetExecutingAssembly().Location;
            _assemblyDir = Path.GetDirectoryName(location).ThrowIfNull();

            _assemblyResolver = new AssemblyResolver(_assemblyDir);
            AppDomain.CurrentDomain.AssemblyResolve += _assemblyResolver.OnAssemblyResolve;

            var ribbonPanel = application.CreateRibbonPanel(Tab.AddIns, "Test Runner");

            var buttonData = new PushButtonData("TestRunner", "Test Runner", Assembly.GetExecutingAssembly().Location, typeof(RunnerCommand).FullName)
            {
                AvailabilityClassName = typeof(AlwaysAvailable).FullName
            };

            ribbonPanel.AddItem(buttonData);

            return Result.Succeeded;
        }
    }
}