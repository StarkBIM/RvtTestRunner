// <copyright file="RvtRunnerLogger.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner
{
    using System.Diagnostics;

    using JetBrains.Annotations;

    using Xunit;

    public class RvtRunnerLogger : IRunnerLogger
    {
        /// <inheritdoc />
        [NotNull]
        public object LockObject { get; } = new object();

        /// <inheritdoc />
        public void LogError(StackFrameInfo stackFrame, [CanBeNull] string message)
        {
            Debug.WriteLine(message);
        }

        /// <inheritdoc />
        public void LogImportantMessage(StackFrameInfo stackFrame, [CanBeNull] string message)
        {
            Debug.WriteLine(message);
        }

        /// <inheritdoc />
        public void LogMessage(StackFrameInfo stackFrame, [CanBeNull] string message)
        {
            Debug.WriteLine(message);
        }

        /// <inheritdoc />
        public void LogWarning(StackFrameInfo stackFrame, [CanBeNull] string message)
        {
            Debug.WriteLine(message);
        }
    }
}