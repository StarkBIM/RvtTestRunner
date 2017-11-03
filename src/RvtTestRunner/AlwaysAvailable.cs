// <copyright file="AlwaysAvailable.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner
{
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using JetBrains.Annotations;

    public class AlwaysAvailable : IExternalCommandAvailability
    {
        /// <inheritdoc />
        public bool IsCommandAvailable([NotNull] UIApplication applicationData, [NotNull] [ItemNotNull] CategorySet selectedCategories)
        {
            return true;
        }
    }
}