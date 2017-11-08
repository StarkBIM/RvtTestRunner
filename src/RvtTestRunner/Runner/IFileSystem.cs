// <copyright file="IFileSystem.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using JetBrains.Annotations;

    internal interface IFileSystem
    {
        [NotNull]
        IFile File { get; }

        [NotNull]
        IDirectory Directory { get; }
    }
}