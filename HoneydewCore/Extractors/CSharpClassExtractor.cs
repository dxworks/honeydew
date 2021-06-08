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
        public CSharpClassExtractor(IList<CSharpMetricExtractor> metricExtractors) : base(metricExtractors)
        {
        }

        public override string FileType()
        {
            return ".cs";
        }

        public override CompilationUnitModel Extract(string fileContent)
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

            IList<ClassModel> entities = new List<ClassModel>();

            CompilationUnitModel compilationUnitModel = new CompilationUnitModel();

            var compilation = CSharpCompilation.Create("Compilation")
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                .AddSyntaxTrees(tree);
            var semanticModel = compilation.GetSemanticModel(tree);

            IList<CSharpMetricExtractor> semanticMetricExtractors = new List<CSharpMetricExtractor>();

            foreach (var metric in MetricExtractors)
            {
                if (metric.GetMetricType() == MetricType.Semantic)
                {
                    metric.SemanticModel = semanticModel;
                    semanticMetricExtractors.Add(metric);
                }
                else
                {
                    metric.Visit(root);
                    compilationUnitModel.SyntacticMetrics.Add(metric.GetName(), metric.GetMetric());
                }
            }

            foreach (var classDeclarationSyntax in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var declaredSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                if (declaredSymbol == null)
                {
                    continue;
                }

                var namespaceSymbol = declaredSymbol.ContainingNamespace;
                var className = declaredSymbol.Name;

                var projectClass = new ClassModel
                {
                    Namespace = namespaceSymbol.ToString(),
                    Name = className
                };

                foreach (var metric in semanticMetricExtractors)
                {
                    metric.Visit(classDeclarationSyntax);
                    projectClass.AddMetric(metric.GetName(), metric.GetMetric());
                }

                entities.Add(projectClass);
            }

            compilationUnitModel.Entities = entities;
            return compilationUnitModel;
        }
    }
}