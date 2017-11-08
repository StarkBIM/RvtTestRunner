// <copyright file="RvtRunnerLogger.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner
{
    using System;
    using System.Diagnostics;

    using JetBrains.Annotations;

    using ReactiveUI;

    using Xunit;

    /// <summary>
    ///     Logs the output
    /// </summary>
    public class RvtRunnerLogger : IRunnerLogger
    {
        [ItemNotNull]
        [NotNull]
        private readonly ReactiveList<string> _allMessages = new ReactiveList<string>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="RvtRunnerLogger" /> class.
        /// </summary>
        /// <param name="stopwatch">The stopwatch used to track elapsed time</param>
        public RvtRunnerLogger([NotNull] Stopwatch stopwatch)
        {
            Stopwatch = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
            AllMessages = _allMessages.CreateDerivedCollection(x => x);
        }

        /// <summary>
        ///     Gets the stopwatch used to track elapsed time
        /// </summary>
        [NotNull]
        public Stopwatch Stopwatch { get; }

        /// <summary>
        ///     Gets a list of all messages produced by the test runner
        /// </summary>
        /// <remarks>
        ///     Using IReactiveDerivedList so that the list is not publicly modifiable, but still provides access to events
        ///     and the full list of messages
        /// </remarks>
        [NotNull]
        [ItemNotNull]
        public IReactiveDerivedList<string> AllMessages { get; }

        /// <inheritdoc />
        [NotNull]
        public object LockObject { get; } = new object();

        /// <inheritdoc />
        public void LogError(StackFrameInfo stackFrame, [CanBeNull] string message)
        {
            var formattedMessage = FormatMessage("Error", message);
            _allMessages.Add(formattedMessage);
            Debug.WriteLine(formattedMessage);
        }

        /// <inheritdoc />
        public void LogImportantMessage(StackFrameInfo stackFrame, [CanBeNull] string message)
        {
            var formattedMessage = FormatMessage("Important", message);
            _allMessages.Add(formattedMessage);
            Debug.WriteLine(formattedMessage);
        }

        /// <inheritdoc />
        public void LogMessage(StackFrameInfo stackFrame, [CanBeNull] string message)
        {
            var formattedMessage = FormatMessage("Message", message);
            _allMessages.Add(formattedMessage);
            Debug.WriteLine(formattedMessage);
        }

        /// <inheritdoc />
        public void LogWarning(StackFrameInfo stackFrame, [CanBeNull] string message)
        {
            var formattedMessage = FormatMessage("Warning", message);
            _allMessages.Add(formattedMessage);
            Debug.WriteLine(formattedMessage);
        }

        [NotNull]
        private string FormatMessage([NotNull] string severity, [CanBeNull] string message)
        {
            return $"[xUnit.net {Stopwatch.Elapsed}] {severity}: {message}";
        }
    }
}