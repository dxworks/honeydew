using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Delegate
{
    public class BaseInfoDelegateVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpDelegateVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, IDelegateType modelType)
        {
            var accessModifier = CSharpConstants.DefaultClassAccessModifier;
            var modifier = "";
            CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier,
                ref modifier);

            var returnType = CSharpHelperMethods.GetFullName(syntaxNode.ReturnType);

            var returnTypeModifier = CSharpHelperMethods.SetTypeModifier(syntaxNode.ReturnType.ToString(), "");

            modelType.Name = CSharpHelperMethods.GetFullName(syntaxNode);
            modelType.AccessModifier = accessModifier;
            modelType.Modifier = modifier;
            modelType.ReturnType = new ReturnTypeModel
            {
                Name = returnType,
                Modifier = returnTypeModifier
            };
            foreach (var parameterType in CSharpHelperMethods.ExtractInfoAboutParameters(syntaxNode.ParameterList))
            {
                modelType.ParameterTypes.Add(parameterType);
            }

            modelType.ClassType = CSharpConstants.DelegateIdentifier;
            modelType.BaseTypes.Add(new BaseTypeModel
            {
                ClassType = CSharpConstants.ClassIdentifier,
                Name = CSharpConstants.SystemDelegate
            });
            modelType.ContainingTypeName = CSharpHelperMethods.GetFullName(syntaxNode)
                .Replace(syntaxNode.Identifier.ToString(), "").Trim('.');

            return modelType;
        }
    }
}
