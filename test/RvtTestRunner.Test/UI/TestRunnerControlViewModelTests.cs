// <copyright file="TestRunnerControlViewModelTests.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Test.UI
{
    using System.Reactive.Concurrency;

    using JetBrains.Annotations;

    using RvtTestRunner.UI;

    using Xunit;

    public class TestRunnerControlViewModelTests
    {
        [Fact]
        public void DefaultValues_Accurate()
        {
            var testRunnerControlViewModel = Build();

            Assert.False(testRunnerControlViewModel.AllowCommandDataAccess);
            Assert.False(testRunnerControlViewModel.IsCopyDllsToNewFolderEnabled);
            Assert.False(testRunnerControlViewModel.CopyDllsToNewFolder);
        }

        [Fact]
        public void IsCopyDllsToNewFolderEnabled_Matches_AllowCommandDataAccess()
        {
            var testRunnerControlViewModel = Build();
            testRunnerControlViewModel.AllowCommandDataAccess = true;

            Assert.True(testRunnerControlViewModel.AllowCommandDataAccess);
            Assert.True(testRunnerControlViewModel.IsCopyDllsToNewFolderEnabled);

            testRunnerControlViewModel.AllowCommandDataAccess = false;

            Assert.False(testRunnerControlViewModel.AllowCommandDataAccess);
            Assert.False(testRunnerControlViewModel.IsCopyDllsToNewFolderEnabled);
        }

        [Fact]
        public void IsCopyDllsToNewFolder_False_When_Disabled()
        {
            var testRunnerControlViewModel = Build();
            testRunnerControlViewModel.AllowCommandDataAccess = true;
            testRunnerControlViewModel.CopyDllsToNewFolder = true;

            Assert.True(testRunnerControlViewModel.CopyDllsToNewFolder);

            testRunnerControlViewModel.AllowCommandDataAccess = false;

            Assert.False(testRunnerControlViewModel.AllowCommandDataAccess);
            Assert.False(testRunnerControlViewModel.CopyDllsToNewFolder);
        }

        [NotNull]
        private static TestRunnerControlViewModel Build() => new TestRunnerControlViewModel(Scheduler.Immediate);
    }
}