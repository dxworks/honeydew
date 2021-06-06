using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors
{
    public class CSharpClassExtractor : Extractor<CSharpMetricExtractor>
    {
        public CSharpClassExtractor(IList<CSharpMetricExtractor> metrics) : base(metrics)
        {
        }

        public override string FileType()
        {
            return ".cs";
        }

        public override ProjectEntity Extract(string fileContent)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                throw new EmptyContentException();
            }

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetCompilationUnitRoot();

            var diagnostics = root.GetDiagnostics();

            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                throw new ExtractionException();
            }

            var namespaceDeclarationSyntax = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First();
            var classDeclarationSyntax = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var projectClass = new ClassModel
            {
                Namespace = namespaceDeclarationSyntax.Name.ToString(),
                Name = classDeclarationSyntax.Identifier.ToString(),
            };

            foreach (var metric in Metrics)
            {
                metric.Visit(root);
                projectClass.Metrics.Add(metric.GetName(), metric.GetMetric());
            }

            return projectClass;
        }
    }
}