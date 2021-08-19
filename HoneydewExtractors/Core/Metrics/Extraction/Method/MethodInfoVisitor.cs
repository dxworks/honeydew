using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Method
{
    public class MethodInfoVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpMethodVisitor
    {
        public IMethodType Visit(MethodDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            var isInterface = false;
            var containingClassName = "";
            if (syntaxNode.Parent != null)
            {
                if (syntaxNode.Parent is InterfaceDeclarationSyntax)
                {
                    isInterface = true;
                }

                if (syntaxNode.Parent is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
                {
                    containingClassName = InheritedSemanticModel.GetFullName(baseTypeDeclarationSyntax);
                }
            }

            GetModifiersForNode(syntaxNode, out var accessModifier, out var modifier, isInterface);

            var returnType = InheritedSemanticModel.GetFullName(syntaxNode.ReturnType);

            var returnTypeModifier = InheritedSyntacticModel.SetTypeModifier(syntaxNode.ReturnType.ToString(), "");


            modelType.Name = syntaxNode.Identifier.ToString();
            modelType.ReturnType = new ReturnTypeModel
            {
                Name = returnType,
                Modifier = returnTypeModifier
            };
            modelType.ContainingTypeName = containingClassName;
            modelType.Modifier = modifier;
            modelType.AccessModifier = accessModifier;
            modelType.CyclomaticComplexity = InheritedSyntacticModel.CalculateCyclomaticComplexity(syntaxNode);

            ExtractInfoAboutParameters(syntaxNode.ParameterList, modelType);

            return modelType;
        }

        private void GetModifiersForNode(MemberDeclarationSyntax node, out string accessModifier, out string modifier,
            bool isInterface)
        {
            var allModifiers = node.Modifiers.ToString();

            accessModifier = isInterface
                ? CSharpConstants.DefaultInterfaceMethodAccessModifier
                : CSharpConstants.DefaultClassMethodAccessModifier;
            modifier = isInterface ? CSharpConstants.DefaultInterfaceMethodModifier : allModifiers;

            CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);
        }

        private void ExtractInfoAboutParameters(BaseParameterListSyntax parameterList, IMethodSignatureType methodModel)
        {
            foreach (var parameter in parameterList.Parameters)
            {
                var parameterType = InheritedSemanticModel.GetFullName(parameter.Type);

                methodModel.ParameterTypes.Add(new ParameterModel
                {
                    Name = parameterType,
                    Modifier = parameter.Modifiers.ToString(),
                    DefaultValue = parameter.Default?.Value.ToString()
                });
            }
        }
    }
}
