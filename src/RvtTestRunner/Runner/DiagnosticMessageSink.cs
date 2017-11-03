// <copyright file="DiagnosticMessageSink.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;

    using Xunit;
    using Xunit.Abstractions;

    public class DiagnosticMessageSink : TestMessageSink
    {
        public DiagnosticMessageSink(Action<MessageHandlerArgs<IDiagnosticMessage>, string> logAction, string assemblyDisplayName, bool showDiagnostics)
        {
            if (showDiagnostics)
            {
                Diagnostics.DiagnosticMessageEvent += args => logAction(args, assemblyDisplayName);
            }
        }
    }
}