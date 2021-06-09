using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors
{
    public class CSharpClassFactExtractor : IFactExtractor
    {
        private readonly IList<CSharpMetricExtractor> _metricExtractors;

        public CSharpClassFactExtractor(IList<CSharpMetricExtractor> metricExtractors)
        {
            _metricExtractors = metricExtractors ?? new List<CSharpMetricExtractor>();
        }

        public string FileType()
        {
            return ".cs";
        }

        public CompilationUnitModel Extract(string fileContent)
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

            var compilationUnitModel = new CompilationUnitModel();

            var compilation = CSharpCompilation.Create("Compilation")
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                .AddSyntaxTrees(tree);
            var semanticModel = compilation.GetSemanticModel(tree);

            IList<CSharpMetricExtractor> semanticMetricExtractors = new List<CSharpMetricExtractor>();

            foreach (var extractor in _metricExtractors)
            {
                if (extractor.GetMetricType() == MetricType.Semantic)
                {
                    extractor.SemanticModel = semanticModel;
                    semanticMetricExtractors.Add(extractor);
                }
                else
                {
                    extractor.Visit(root);
                    compilationUnitModel.SyntacticMetrics.Add(extractor);
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

                foreach (var extractor in semanticMetricExtractors)
                {
                    extractor.Visit(classDeclarationSyntax);
                    projectClass.Metrics.Add(extractor);
                }

                entities.Add(projectClass);
            }

            compilationUnitModel.ClassModels = entities;
            return compilationUnitModel;
        }
    }
}