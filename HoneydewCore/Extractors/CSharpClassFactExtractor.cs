using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
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

        public IList<ClassModel> Extract(SyntaxTree tree)
        {
            var root = GetCompilationUnitSyntaxTree(tree);

            IList<ClassModel> classModels = new List<ClassModel>();

            var semanticModel = CreateSemanticModel(tree);

            var syntaxes = GetClassAndInterfaceSyntaxes(root);

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
                    FullName = $"{namespaceSymbol}.{className}",
                    Fields = ExtractFieldsInfo(declarationSyntax),
                    Methods = ExtractMethodInfo(declarationSyntax, semanticModel)
                };


                foreach (var extractorType in _metricExtractorsTypes)
                {
                    var extractor = (CSharpMetricExtractor) Activator.CreateInstance(extractorType);
                    if (extractor == null)
                    {
                        continue;
                    }

                    if (extractor is ISemanticMetric)
                    {
                        extractor.SemanticModel = semanticModel;
                    }

                    if (extractor is ICompilationUnitMetric)
                    {
                        extractor.Visit(root);
                    }
                    else
                    {
                        extractor.Visit(declarationSyntax);
                    }

                    var metric = extractor.GetMetric();
                    projectClass.Metrics.Add(new ClassMetric
                    {
                        ExtractorName = extractorType.FullName,
                        Value = metric.GetValue(),
                        ValueType = metric.GetValueType()
                    });
                }

                classModels.Add(projectClass);
            }

            return classModels;
        }

        public IList<ClassModel> Extract(string fileContent)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                throw new EmptyContentException();
            }

            var tree = CSharpSyntaxTree.ParseText(fileContent);

            return Extract(tree);
        }

        private static SemanticModel CreateSemanticModel(SyntaxTree tree)
        {
            var compilation = CSharpCompilation.Create("Compilation")
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                .AddSyntaxTrees(tree);
            var semanticModel = compilation.GetSemanticModel(tree);
            return semanticModel;
        }

        private static IList<TypeDeclarationSyntax> GetClassAndInterfaceSyntaxes(CompilationUnitSyntax root)
        {
            var classDeclarationSyntaxes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            var interfaceDeclarationSyntaxes = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            var recordDeclarationSyntaxes = root.DescendantNodes().OfType<RecordDeclarationSyntax>();
            var structDeclarationSyntaxes = root.DescendantNodes().OfType<StructDeclarationSyntax>();

            IList<TypeDeclarationSyntax> syntaxes = classDeclarationSyntaxes.Cast<TypeDeclarationSyntax>().ToList();

            foreach (var syntax in interfaceDeclarationSyntaxes)
            {
                syntaxes.Add(syntax);
            }

            foreach (var syntax in recordDeclarationSyntaxes)
            {
                syntaxes.Add(syntax);
            }

            foreach (var syntax in structDeclarationSyntaxes)
            {
                syntaxes.Add(syntax);
            }

            return syntaxes;
        }

        private static CompilationUnitSyntax GetCompilationUnitSyntaxTree(SyntaxTree tree)
        {
            var root = tree.GetCompilationUnitRoot();

            var diagnostics = root.GetDiagnostics();

            var enumerable = diagnostics as Diagnostic[] ?? diagnostics.ToArray();
            if (diagnostics != null && enumerable.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                var result = enumerable.Aggregate("", (current, diagnostic) => current + diagnostic);
                throw new ExtractionException(result);
            }

            return root;
        }

        private static IList<FieldModel> ExtractFieldsInfo(SyntaxNode declarationSyntax)
        {
            var fieldsInfoMetric = new FieldsInfoMetric();
            fieldsInfoMetric.Visit(declarationSyntax);
            return fieldsInfoMetric.FieldInfos;
        }

        private static IList<MethodModel> ExtractMethodInfo(SyntaxNode declarationSyntax, SemanticModel semanticModel)
        {
            var fieldsInfoMetric = new MethodInfoMetric
            {
                SemanticModel = semanticModel
            };
            fieldsInfoMetric.Visit(declarationSyntax);
            return fieldsInfoMetric.MethodInfos;
        }
    }
}