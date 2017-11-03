// <copyright file="TestRunnerControlViewModel.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.UI
{
    using System.IO;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Input;

    using JetBrains.Annotations;

    using Ookii.Dialogs.Wpf;

    using ReactiveUI;

    public class TestRunnerControlViewModel : ReactiveObject
    {
        [CanBeNull]
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
                                                           var vistaOpenFileDialog = new VistaOpenFileDialog()
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

            var canRemove = this.WhenAnyValue(x => x.SelectedAssembly, assembly => !assembly.IsNullOrWhiteSpace());

            RemoveCommand = ReactiveCommand.Create(
                                                   () =>
                                                       {
                                                           SelectedAssemblies.Remove(SelectedAssembly);

                                                       },
                                                   canRemove);

            CancelCommand = ReactiveCommand.Create((Window window) => window.Close());

            var canExecute = this.WhenAnyObservable(x => x.SelectedAssemblies.CountChanged).Select(count => count > 0);

            ExecuteCommand = ReactiveCommand.Create((Window window) => window.Close(), canExecute);
        }

        [NotNull]
        public ICommand RemoveCommand { get; }

        [NotNull]
        public ICommand BrowseCommand { get; }

        [NotNull]
        public ICommand CancelCommand { get; }

        [NotNull]
        public ICommand ExecuteCommand { get; }

        [NotNull]
        [ItemNotNull]
        public ReactiveList<string> SelectedAssemblies { get; } = new ReactiveList<string>();

        [CanBeNull]
        public string SelectedAssembly
        {
            get => _selectedAssembly;
            set => this.RaiseAndSetIfChanged(ref _selectedAssembly, value);
        }
    }
}