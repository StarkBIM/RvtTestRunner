// <copyright file="IFileSystem.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    internal interface IFileSystem
    {
        IFile File { get; }
        IDirectory Directory { get; }
    }
}