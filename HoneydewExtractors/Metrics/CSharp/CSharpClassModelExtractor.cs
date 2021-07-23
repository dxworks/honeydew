using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp;
using HoneydewExtractors.Utils;
using HoneydewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Metrics.CSharp
{
    public class CSharpClassModelExtractor : IClassModelExtractor<ClassModel, CSharpSyntacticModel, CSharpSemanticModel,
        CSharpSyntaxNode>
    {
        public IList<ClassModel> Extract(CSharpSyntacticModel syntacticModel, CSharpSemanticModel semanticModel,
            MetricLoader<IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>> metricLoader)
        {
            IList<ClassModel> classModels = new List<ClassModel>();

            var root = syntacticModel.CompilationUnitSyntax;
            var syntaxes = root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>().ToList();

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
                var projectClass = new ClassModel
                {
                    ClassType = classType,
                    AccessModifier = accessModifier,
                    Modifier = modifier,
                    FullName = fullName,
                    Fields = ExtractFieldsInfo(declarationSyntax, semanticModel),
                    Methods = methodInfoDataMetric.MethodInfos,
                    Constructors = methodInfoDataMetric.ConstructorInfos,
                    BaseClassFullName = baseClassName,
                    BaseInterfaces = baseInterfaces
                };

                var extractionMetrics = metricLoader.InstantiateMetrics();

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

                    projectClass.AddMetricValue(extractionMetric.GetType().FullName, extractionMetric.GetMetric());
                }


                classModels.Add(projectClass);
            }

            return classModels;
        }

        private static IList<FieldModel> ExtractFieldsInfo(SyntaxNode declarationSyntax,
            CSharpSemanticModel semanticModel)
        {
            // var fieldsInfoMetric = new FieldsInfoMetric
            // {
            //     ExtractorSemanticModel = semanticModel
            // };
            // fieldsInfoMetric.Visit(declarationSyntax);
            // return fieldsInfoMetric.FieldInfos;
            return new List<FieldModel>();
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

        private void ExtractBaseClassAndBaseInterfaces(SyntaxNode declarationSyntax,
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
