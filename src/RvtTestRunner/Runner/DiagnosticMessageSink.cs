// <copyright file="DiagnosticMessageSink.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;

    using JetBrains.Annotations;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    ///     The message sink for diagnostic messages. Logs diagnostic messages with the action supplied to this object
    /// </summary>
    public class DiagnosticMessageSink : TestMessageSink
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DiagnosticMessageSink" /> class.
        /// </summary>
        /// <param name="logAction">The log action</param>
        /// <param name="assemblyDisplayName">The display name for the assembly</param>
        /// <param name="showDiagnostics">
        ///     A value indicating whether diagnostics should be down. This class does nothing if false
        ///     is passed
        /// </param>
        public DiagnosticMessageSink([NotNull] Action<MessageHandlerArgs<IDiagnosticMessage>, string> logAction, [NotNull] string assemblyDisplayName, bool showDiagnostics)
        {
            if (showDiagnostics)
            {
                Diagnostics.DiagnosticMessageEvent += args => logAction(args, assemblyDisplayName);
            }
        }
    }
}