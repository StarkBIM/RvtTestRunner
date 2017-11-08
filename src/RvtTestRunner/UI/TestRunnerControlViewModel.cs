// <copyright file="TestRunnerControlViewModel.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.UI
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Input;

    using JetBrains.Annotations;

    using Ookii.Dialogs.Wpf;

    using ReactiveUI;

    using RvtTestRunner.Util;

    /// <summary>
    /// ViewModel for the TestRunner control
    /// </summary>
    public class TestRunnerControlViewModel : ReactiveObject
    {
        [CanBeNull]
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Analyzer bug. The field is used")]
        private string _selectedAssembly;

        [CanBeNull]
        private string _lastSelectedFolder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestRunnerControlViewModel" /> class.
        /// </summary>
        public TestRunnerControlViewModel()
        {
            BrowseCommand = ReactiveCommand.Create(
                                                   () =>
                                                       {
                                                           var vistaOpenFileDialog = new VistaOpenFileDialog
                                                               {
                                                                   Multiselect = true,
                                                                   Filter = "DLL files (*.dll)|*.dll"
                                                               };

                                                           if (!_lastSelectedFolder.IsNullOrWhiteSpace())
                                                           {
                                                               vistaOpenFileDialog.InitialDirectory = _lastSelectedFolder;
                                                           }

                                                           var result = vistaOpenFileDialog.ShowDialog();

                                                           if (result != true)
                                                           {
                                                               return;
                                                           }

                                                           _lastSelectedFolder = Path.GetDirectoryName(vistaOpenFileDialog.FileName);

                                                           foreach (var fileName in vistaOpenFileDialog.FileNames)
                                                           {
                                                               if (!SelectedAssemblies.Contains(fileName))
                                                               {
                                                                   SelectedAssemblies.Add(fileName);
                                                               }
                                                           }
                                                       });

            var canRemove = this.WhenAnyValue(x => x.SelectedAssembly, assembly => !assembly.IsNullOrWhiteSpace()).ObserveOnDispatcher();

            RemoveCommand = ReactiveCommand.Create(
                                                   () =>
                                                       {
                                                           SelectedAssemblies.Remove(SelectedAssembly);
                                                       },
                                                   canRemove);

            CancelCommand = ReactiveCommand.Create((Window window) =>
                {
                    window.DialogResult = false;
                    window.Close();
                });

            var canExecute = this.WhenAnyObservable(x => x.SelectedAssemblies.CountChanged).ObserveOnDispatcher().Select(count => count > 0);

            ExecuteCommand = ReactiveCommand.Create(
                (Window window) =>
                {
                    window.DialogResult = true;
                    window.Close();
                }, canExecute);
        }

        /// <summary>
        /// Gets the command that removes the selected assembly from the list
        /// </summary>
        [NotNull]
        public ICommand RemoveCommand { get; }

        /// <summary>
        /// Gets the command that opens a file browser window that allows assemblies to be added to the list
        /// </summary>
        [NotNull]
        public ICommand BrowseCommand { get; }

        /// <summary>
        /// Gets the command that cancels the test running process and closes the window
        /// </summary>
        [NotNull]
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Gets the command that executes the tests in the selected assemblies and closes the window
        /// </summary>
        [NotNull]
        public ICommand ExecuteCommand { get; }

        /// <summary>
        /// Gets the list of selected assemblies
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public ReactiveList<string> SelectedAssemblies { get; } = new ReactiveList<string>();

        /// <summary>
        /// Gets or sets the currently selected assembly
        /// </summary>
        [CanBeNull]
        public string SelectedAssembly
        {
            get => _selectedAssembly;
            set => this.RaiseAndSetIfChanged(ref _selectedAssembly, value);
        }
    }
}