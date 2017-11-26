// <copyright file="IDirectory.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System.Diagnostics.CodeAnalysis;

    using JetBrains.Annotations;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "xUnit class")]
    internal interface IDirectory
    {
        bool Exists([NotNull] string path);
    }
}