// <copyright file="Transform.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    using JetBrains.Annotations;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "xUnit class")]
    public class Transform
    {
        [NotNull]
        public string CommandLine { get; set; }

        [NotNull]
        public string Description { get; set; }

        [NotNull]
        public Action<XElement, string> OutputHandler { get; set; }
    }
}