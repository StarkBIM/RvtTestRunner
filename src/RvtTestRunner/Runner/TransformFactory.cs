// <copyright file="TransformFactory.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Xsl;

    using JetBrains.Annotations;

    using RvtTestRunner.Util;

    using Xunit;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "xUnit class")]
    public class TransformFactory
    {
        [NotNull]
        private static readonly TransformFactory Instance = new TransformFactory();

        [NotNull]
        private readonly Dictionary<string, Transform> _availableTransforms = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);

        private TransformFactory()
        {
            _availableTransforms.Add(
                                     "xml",
                                     new Transform
                                     {
                                         CommandLine = "xml",
                                         Description = "output results to xUnit.net v2 XML file",
                                         OutputHandler = Handler_DirectWrite
                                     });
            _availableTransforms.Add(
                                     "xmlv1",
                                     new Transform
                                     {
                                         CommandLine = "xmlv1",
                                         Description = "output results to xUnit.net v1 XML file",
                                         OutputHandler = (xml, outputFileName) => Handler_XslTransform("xUnit1.xslt", xml, outputFileName)
                                     });
            _availableTransforms.Add(
                                     "html",
                                     new Transform
                                     {
                                         CommandLine = "html",
                                         Description = "output results to HTML file",
                                         OutputHandler = (xml, outputFileName) => Handler_XslTransform("HTML.xslt", xml, outputFileName)
                                     });
            _availableTransforms.Add(
                                     "nunit",
                                     new Transform
                                     {
                                         CommandLine = "nunit",
                                         Description = "output results to NUnit v2.5 XML file",
                                         OutputHandler = (xml, outputFileName) => Handler_XslTransform("NUnitXml.xslt", xml, outputFileName)
                                     });
        }

        [NotNull]
        [ItemNotNull]
        public static List<Transform> AvailableTransforms
            => Instance._availableTransforms.Values.ToList();

        [NotNull]
        [ItemNotNull]
        public static List<Action<XElement>> GetXmlTransformers([NotNull] [ItemNotNull] XunitProject project)
            => project.Output
                .Select(output => new Action<XElement>(xml => Instance._availableTransforms[output.Key].OutputHandler(xml, output.Value)))
                .ToList();

        private static void Handler_DirectWrite([NotNull] XElement xml, [NotNull] string outputFileName)
        {
            using (var stream = File.Create(outputFileName))
            {
                xml.Save(stream);
            }
        }

        private static void Handler_XslTransform([NotNull] string resourceName, [NotNull] XElement xml, [NotNull] string outputFileName)
        {
            var xmlTransform = new XslCompiledTransform();

            var settings = new XmlReaderSettings();

            using (var writer = XmlWriter.Create(
                                                 outputFileName,
                                                 new XmlWriterSettings
                                                 {
                                                     Indent = true
                                                 }))
            using (var xsltStream = typeof(TransformFactory).GetTypeInfo().Assembly.GetManifestResourceStream($"Xunit.ConsoleClient.{resourceName}"))
            using (var xsltReader = XmlReader.Create(xsltStream.ThrowIfNull(), settings))
            using (var xmlReader = xml.CreateReader())
            {
                xmlTransform.Load(xsltReader);
                xmlTransform.Transform(xmlReader, writer);
            }
        }
    }
}