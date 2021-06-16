using System;
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
        private readonly IList<Type> _metricExtractorsTypes;

        public CSharpClassFactExtractor()
        {
            _metricExtractorsTypes = new List<Type>();
        }

        public CSharpClassFactExtractor(IList<Type> metricExtractors)
        {
            _metricExtractorsTypes = metricExtractors ?? new List<Type>();
        }

        public void AddMetric<T>() where T : CSharpMetricExtractor
        {
            _metricExtractorsTypes.Add(typeof(T));
        }

        public string FileType()
        {
            return ".cs";
        }

        public IList<ProjectClassModel> Extract(string fileContent)
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

            IList<ProjectClassModel> classModels = new List<ProjectClassModel>();


            var compilation = CSharpCompilation.Create("Compilation")
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                .AddSyntaxTrees(tree);
            var semanticModel = compilation.GetSemanticModel(tree);


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

                var projectClass = new ProjectClassModel()
                {
                    FullName = namespaceSymbol.ToString() + "." + className
                };

                foreach (var extractorType in _metricExtractorsTypes)
                {
                    var extractor = (CSharpMetricExtractor) Activator.CreateInstance(extractorType);

                    if (extractor is ISyntacticMetric)
                    {
                        extractor.Visit(root);
                        
                        var metric = extractor.GetMetric();
                        projectClass.Metrics.Add(new ClassMetric
                        {
                            ExtractorName = extractorType.FullName,
                            Value = metric.GetValue(),
                            ValueType = metric.GetValueType()
                        });
                    }

                    if (extractor is ISemanticMetric)
                    {
                        extractor.SemanticModel = semanticModel;
                        if (extractor is not ISyntacticMetric)
                        {
                            extractor.Visit(declarationSyntax);
                            
                            var metric = extractor.GetMetric();
                            projectClass.Metrics.Add(new ClassMetric
                            {
                                ExtractorName = extractorType.FullName,
                                Value = metric.GetValue(),
                                ValueType = metric.GetValueType()
                            });
                        }
                    }
                }

                classModels.Add(projectClass);
            }

            return classModels;
        }
    }
}