// <copyright file="IFile.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System.IO;
    using JetBrains.Annotations;

    internal interface IFile
    {
        bool Exists([NotNull] string path);

        [NotNull]
        string ReadAllText([NotNull] string path);

        [NotNull]
        Stream OpenRead([NotNull] string path);

        [NotNull]
        Stream OpenFile(
            [NotNull] string path,
            FileMode fileMode,
            FileAccess fileAccess,
            FileShare fileShare,
            int bufferSize,
            FileOptions fileOptions);

        void CreateEmptyFile([NotNull] string path);
    }
}