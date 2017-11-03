// <copyright file="RvtUnitTestAttribute.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner
{
    using System;

    /// <summary>
    ///     Indicates that a unit test needs to be run in Revit, through the RvtTestRunner
    ///     Any test with this attribute should be excluded from automated test runs, such as Live Unit Testing, any JetBrains
    ///     automatic testing products, and test runs during CI builds
    ///     The ExcludeFromCodeCoverage attribute should be applied to any test that has this attribute
    ///     A way of running these tests during CI builds is being investigated
    ///     See
    ///     https://blogs.msdn.microsoft.com/visualstudio/2017/03/09/live-unit-testing-in-visual-studio-2017-enterprise/#faqs
    ///     "Q: How do I exclude tests from participating in Live Unit Testing?"
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RvtUnitTestAttribute : Attribute
    {
    }
}