using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Models;
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

        public IList<ClassModel> Extract(string fileContent)
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

            IList<ClassModel> classModels = new List<ClassModel>();


            var compilation = CSharpCompilation.Create("Compilation")
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                .AddSyntaxTrees(tree);
            var semanticModel = compilation.GetSemanticModel(tree);

            IList<CSharpMetricExtractor> semanticMetricExtractors = new List<CSharpMetricExtractor>();

            var metricsSet = new MetricsSet();

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
                    metricsSet.Add(extractor);
                }
            }

            var classDeclarationSyntaxes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            var interfaceDeclarationSyntaxes = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();

            IList<TypeDeclarationSyntax> syntaxes = classDeclarationSyntaxes.Cast<TypeDeclarationSyntax>().ToList();

            foreach (var syntax in interfaceDeclarationSyntaxes)
            {
                syntaxes.Add(syntax);
            }

            foreach (var declarationSyntax in syntaxes)
            {
                var declaredSymbol = semanticModel.GetDeclaredSymbol(declarationSyntax);
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
                    extractor.Visit(declarationSyntax);
                    projectClass.Metrics.Add(extractor);
                }

                classModels.Add(projectClass);
            }
            

            foreach (var model in classModels)
            {
                foreach (var (extractorType, metric) in metricsSet.Metrics)
                {
                    model.Metrics.AddValue(extractorType, metric);
                }
            }

            return classModels;
        }
    }
}