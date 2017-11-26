// <copyright file="IDependencyContextReader.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyModel;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "xUnit class")]
    internal interface IDependencyContextReader : IDisposable
    {
        [NotNull]
        DependencyContext Read([NotNull] Stream stream);
    }
}