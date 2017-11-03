// <copyright file="LongLivedMarshalByRefObject.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Security;

    /// <summary>
    /// Base class for all long-lived objects that may cross over an AppDomain.
    /// </summary>
    public abstract class LongLivedMarshalByRefObject : MarshalByRefObject
    {
        /// <inheritdoc/>
        [SecurityCritical]
        public sealed override object InitializeLifetimeService()
        {
            return null;
        }
    }
}