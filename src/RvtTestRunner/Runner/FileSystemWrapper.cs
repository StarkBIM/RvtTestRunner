// <copyright file="FileSystemWrapper.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using JetBrains.Annotations;

    internal class FileSystemWrapper : IFileSystem
    {
        [NotNull]
        public static IFileSystem Default { get; } = new FileSystemWrapper();

        public IFile File { get; } = new FileWrapper();

        public IDirectory Directory { get; } = new DirectoryWrapper();
    }
}