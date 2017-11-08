// <copyright file="IDirectory.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using JetBrains.Annotations;

    internal interface IDirectory
    {
        bool Exists([NotNull] string path);
    }
}