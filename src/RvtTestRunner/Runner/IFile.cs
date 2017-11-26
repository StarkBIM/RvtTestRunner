// <copyright file="IFile.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    using JetBrains.Annotations;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "xUnit class")]
    internal interface IFile
    {
        void CreateEmptyFile([NotNull] string path);

        bool Exists([NotNull] string path);

        [NotNull]
        Stream OpenFile(
            [NotNull] string path,
            FileMode fileMode,
            FileAccess fileAccess,
            FileShare fileShare,
            int bufferSize,
            FileOptions fileOptions);

        [NotNull]
        Stream OpenRead([NotNull] string path);

        [NotNull]
        string ReadAllText([NotNull] string path);
    }
}