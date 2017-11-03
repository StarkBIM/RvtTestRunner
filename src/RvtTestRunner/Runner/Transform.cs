// <copyright file="Transform.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Xml.Linq;

    public class Transform
    {
        public string CommandLine { get; set; }
        public string Description { get; set; }
        public Action<XElement, string> OutputHandler { get; set; }
    }
}