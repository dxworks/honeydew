﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
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

                ExtractBaseClassAndBaseInterfaces(declarationSyntax, semanticModel, out var baseClassName,
                    out var baseInterfaces);

                var projectClass = new ClassModel
                {
                    FullName = $"{namespaceSymbol}.{className}",
                    Fields = ExtractFieldsInfo(declarationSyntax, semanticModel),
                    Methods = ExtractMethodInfo(declarationSyntax, semanticModel),
                    BaseClassFullName = baseClassName,
                    BaseInterfaces = baseInterfaces
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
                        extractor.ExtractorSemanticModel = semanticModel;
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
                        ValueType = metric.GetValueType(),
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
            var compilation = CSharpCompilation.Create("Compilation");

            // try to add a reference to the System assembly
            var systemReference = typeof(object).Assembly.Location;

            // if 'systemReference' is empty means that the build is a single-file app and should look in the dlls to search for the System.dll 
            if (string.IsNullOrEmpty(systemReference))
            {
                var value = (string) AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
                if (value != null)
                {
                    var pathToDlls = value.Split(Path.PathSeparator);
                    var pathToSystem = pathToDlls.FirstOrDefault(path => path.Contains("System.dll"));

                    if (!string.IsNullOrEmpty(pathToSystem))
                    {
                        compilation = compilation.AddReferences(MetadataReference.CreateFromFile(pathToSystem));
                    }
                }
            }
            // if 'systemReference' is empty means that the System.dll Location is accessible with Reflection
            else
            {
                compilation = compilation.AddReferences(MetadataReference.CreateFromFile(systemReference));
            }

            compilation = compilation.AddSyntaxTrees(tree);

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

        private static IList<FieldModel> ExtractFieldsInfo(SyntaxNode declarationSyntax, SemanticModel semanticModel)
        {
            var fieldsInfoMetric = new FieldsInfoMetric
            {
                ExtractorSemanticModel = semanticModel
            };
            fieldsInfoMetric.Visit(declarationSyntax);
            return fieldsInfoMetric.FieldInfos;
        }

        private static IList<MethodModel> ExtractMethodInfo(SyntaxNode declarationSyntax, SemanticModel semanticModel)
        {
            var fieldsInfoMetric = new MethodInfoMetric
            {
                ExtractorSemanticModel = semanticModel
            };
            fieldsInfoMetric.Visit(declarationSyntax);
            return fieldsInfoMetric.MethodInfos;
        }

        private void ExtractBaseClassAndBaseInterfaces(SyntaxNode declarationSyntax,
            SemanticModel semanticModel, out string baseClass, out IList<string> baseInterfaces)
        {
            var fieldsInfoMetric = new BaseClassMetric
            {
                ExtractorSemanticModel = semanticModel
            };
            fieldsInfoMetric.Visit(declarationSyntax);

            baseClass = fieldsInfoMetric.InheritanceMetric.BaseClassName;
            baseInterfaces = fieldsInfoMetric.InheritanceMetric.Interfaces;
        }
    }
}