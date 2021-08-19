using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Delegate
{
    public class BaseInfoDelegateVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpDelegateVisitor
    {
        public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, IDelegateType modelType)
        {
            var accessModifier = CSharpConstants.DefaultClassAccessModifier;
            var modifier = "";
            CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier,
                ref modifier);

            var returnType = InheritedSemanticModel.GetFullName(syntaxNode.ReturnType);

            var returnTypeModifier = InheritedSyntacticModel.SetTypeModifier(syntaxNode.ReturnType.ToString(), "");

            modelType.Name = InheritedSemanticModel.GetFullName(syntaxNode);
            modelType.AccessModifier = accessModifier;
            modelType.Modifier = modifier;
            modelType.ReturnType = new ReturnTypeModel
            {
                Name = returnType,
                Modifier = returnTypeModifier
            };
            foreach (var parameterType in InheritedSemanticModel.ExtractInfoAboutParameters(syntaxNode.ParameterList))
            {
                modelType.ParameterTypes.Add(parameterType);
            }

            modelType.ClassType = CSharpConstants.DelegateIdentifier;
            modelType.BaseTypes.Add(new BaseTypeModel
            {
                ClassType = CSharpConstants.ClassIdentifier,
                Name = CSharpConstants.SystemDelegate
            });


            return modelType;
        }
    }
}
