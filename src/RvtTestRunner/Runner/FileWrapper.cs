// <copyright file="FileWrapper.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "xUnit class")]
    internal class FileWrapper : IFile
    {
        public void CreateEmptyFile(string path)
        {
            try
            {
                var emptyFile = File.Create(path);
                emptyFile.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        public bool Exists(string path) => File.Exists(path);

        public Stream OpenFile(
            string path,
            FileMode fileMode,
            FileAccess fileAccess,
            FileShare fileShare,
            int bufferSize,
            FileOptions fileOptions) => new FileStream(path, fileMode, fileAccess, fileShare, bufferSize, fileOptions);

        public Stream OpenRead(string path) => File.OpenRead(path);

        public string ReadAllText(string path) => File.ReadAllText(path);
    }
}