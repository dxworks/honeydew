using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Utils;
using HoneydewExtractors.Core;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpClassModelExtractor : IClassModelExtractor<ClassModel, CSharpSyntacticModel, CSharpSemanticModel,
        CSharpSyntaxNode>
    {
        public IList<ClassModel> Extract(CSharpSyntacticModel syntacticModel, CSharpSemanticModel semanticModel,
            TypeSpawner<IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>> metricSpawner)
        {
            IList<ClassModel> classModels = new List<ClassModel>();

            var root = syntacticModel.CompilationUnitSyntax;
            var syntaxes = new List<MemberDeclarationSyntax>();

            syntaxes.AddRange(root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>().ToList());
            syntaxes.AddRange(root.DescendantNodes().OfType<DelegateDeclarationSyntax>());

            CSharpLinesOfCodeCounter linesOfCodeCounter = new();


            foreach (var declarationSyntax in syntaxes)
            {
                var fullName = semanticModel.GetFullName(declarationSyntax);

                ExtractBaseClassAndBaseInterfaces(declarationSyntax, semanticModel, out var baseClassName,
                    out var baseInterfaces);

                var classType = declarationSyntax.Kind().ToString().Replace("Declaration", "").ToLower();

                var accessModifier = CSharpConstants.DefaultClassAccessModifier;
                var modifier = "";
                CSharpConstants.SetModifiers(declarationSyntax.Modifiers.ToString(), ref accessModifier, ref modifier);

                var methodInfoDataMetric = ExtractMethodInfo(declarationSyntax, semanticModel);

                var linesOfCode = linesOfCodeCounter.Count(syntaxes.Count == 1
                    ? root.GetText().ToString()
                    : declarationSyntax.ToString());


                var classModel = new ClassModel
                {
                    ClassType = classType,
                    AccessModifier = accessModifier,
                    Modifier = modifier,
                    FullName = fullName,
                    Fields = ExtractFieldsInfo(declarationSyntax, semanticModel),
                    Properties = ExtractPropertiesInfo(declarationSyntax, semanticModel),
                    Methods = methodInfoDataMetric.MethodInfos,
                    Constructors = methodInfoDataMetric.ConstructorInfos,
                    BaseClassFullName = baseClassName,
                    BaseInterfaces = baseInterfaces,
                    Loc = linesOfCode
                };

                var extractionMetrics = metricSpawner.InstantiateMetrics();

                foreach (var extractionMetric in extractionMetrics)
                {
                    extractionMetric.HoneydewSemanticModel = semanticModel;
                    extractionMetric.HoneydewSyntacticModel = syntacticModel;

                    if (extractionMetric.GetMetricType() == ExtractionMetricType.CompilationUnitLevel)
                    {
                        extractionMetric.Visit(new CSharpSyntaxNode(root));
                    }
                    else
                    {
                        extractionMetric.Visit(new CSharpSyntaxNode(declarationSyntax));
                    }

                    classModel.AddMetricValue(extractionMetric.GetType().FullName, extractionMetric.GetMetric());
                }

                classModels.Add(classModel);
            }

            var usingsMetric = new CSharpUsingsMetric
            {
                HoneydewSemanticModel = semanticModel
            };
            usingsMetric.Visit(root);
            foreach (var classModel in classModels)
            {
                if (usingsMetric.Usings.TryGetValue(classModel.FullName, out var usings))
                {
                    classModel.Usings = usings.ToList();
                }
            }

            return classModels;
        }

        private static IList<PropertyModel> ExtractPropertiesInfo(SyntaxNode declarationSyntax,
            CSharpSemanticModel semanticModel)
        {
            var fieldsInfoMetric = new CSharpPropertiesInfoMetric
            {
                HoneydewSemanticModel = semanticModel
            };
            fieldsInfoMetric.Visit(declarationSyntax);
            return fieldsInfoMetric.PropertyInfos;
        }

        private static IList<FieldModel> ExtractFieldsInfo(SyntaxNode declarationSyntax,
            CSharpSemanticModel semanticModel)
        {
            var fieldsInfoMetric = new CSharpFieldsInfoMetric
            {
                HoneydewSemanticModel = semanticModel
            };
            fieldsInfoMetric.Visit(declarationSyntax);
            return fieldsInfoMetric.FieldInfos;
        }

        private static CSharpMethodInfoDataMetric ExtractMethodInfo(SyntaxNode declarationSyntax,
            CSharpSemanticModel semanticModel)
        {
            var fieldsInfoMetric = new CSharpMethodInfoMetric
            {
                HoneydewSemanticModel = semanticModel
            };
            fieldsInfoMetric.Visit(declarationSyntax);
            return fieldsInfoMetric.DataMetric;
        }

        private static void ExtractBaseClassAndBaseInterfaces(SyntaxNode declarationSyntax,
            CSharpSemanticModel semanticModel, out string baseClass, out IList<string> baseInterfaces)
        {
            var fieldsInfoMetric = new CSharpBaseClassMetric
            {
                HoneydewSemanticModel = semanticModel
            };
            fieldsInfoMetric.Visit(declarationSyntax);

            baseClass = fieldsInfoMetric.InheritanceMetric.BaseClassName;
            baseInterfaces = fieldsInfoMetric.InheritanceMetric.Interfaces;
        }
    }
}
