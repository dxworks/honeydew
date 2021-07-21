using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Utils;
using HoneydewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp
{
    public class CSharpClassModelExtractor : IClassModelExtractor
    {
        public IList<IClassModel> Extract(ISyntacticModel syntacticModel, ISemanticModel semModel)
        {
            IList<IClassModel> classModels = new List<IClassModel>();

            var root = ((CSharpSyntacticModel) syntacticModel).CompilationUnitSyntax;
            var semanticModel = ((CSharpSemanticModel) semModel).Model;
            var syntaxes = root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>().ToList();

            foreach (var declarationSyntax in syntaxes)
            {
                var declaredSymbol = semanticModel.GetDeclaredSymbol(declarationSyntax);
                if (declaredSymbol == null)
                {
                    continue;
                }

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
                    FullName = declaredSymbol.ToDisplayString(),
                    Fields = ExtractFieldsInfo(declarationSyntax, semanticModel),
                    Methods = methodInfoDataMetric.MethodInfos,
                    Constructors = methodInfoDataMetric.ConstructorInfos,
                    BaseClassFullName = baseClassName,
                    BaseInterfaces = baseInterfaces
                };

                classModels.Add(projectClass);
            }

            return classModels;
        }

        private static IList<FieldModel> ExtractFieldsInfo(SyntaxNode declarationSyntax, SemanticModel semanticModel)
        {
            // var fieldsInfoMetric = new FieldsInfoMetric
            // {
            //     ExtractorSemanticModel = semanticModel
            // };
            // fieldsInfoMetric.Visit(declarationSyntax);
            // return fieldsInfoMetric.FieldInfos;
            return new List<FieldModel>();
        }

        private static MethodInfoDataMetric ExtractMethodInfo(SyntaxNode declarationSyntax, SemanticModel semanticModel)
        {
            // var fieldsInfoMetric = new MethodInfoMetric
            // {
            //     ExtractorSemanticModel = semanticModel
            // };
            // fieldsInfoMetric.Visit(declarationSyntax);
            // return fieldsInfoMetric.DataMetric;
            return new();
        }

        private void ExtractBaseClassAndBaseInterfaces(SyntaxNode declarationSyntax,
            SemanticModel semanticModel, out string baseClass, out IList<string> baseInterfaces)
        {
            // var fieldsInfoMetric = new BaseClassMetric
            // {
            //     ExtractorSemanticModel = semanticModel
            // };
            // fieldsInfoMetric.Visit(declarationSyntax);
            //
            // baseClass = fieldsInfoMetric.InheritanceMetric.BaseClassName;
            // baseInterfaces = fieldsInfoMetric.InheritanceMetric.Interfaces;
            baseClass = "";
            baseInterfaces = new List<string>();
        }
    }

    internal class MethodInfoDataMetric
    {
        public IList<MethodModel> MethodInfos { get; set; }
        public IList<MethodModel> ConstructorInfos { get; set; }
    }
}
