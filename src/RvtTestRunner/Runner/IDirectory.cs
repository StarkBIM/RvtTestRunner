// <copyright file="IDirectory.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    internal interface IDirectory
    {
        bool Exists(string path);
    }
}