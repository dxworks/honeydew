using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Method
{
    public class MethodInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpMethodVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

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
                    containingClassName = CSharpHelperMethods.GetFullName(baseTypeDeclarationSyntax);
                }
            }

            GetModifiersForNode(syntaxNode, out var accessModifier, out var modifier, isInterface);

            var returnType = CSharpHelperMethods.GetFullName(syntaxNode.ReturnType);

            var returnTypeModifier = CSharpHelperMethods.SetTypeModifier(syntaxNode.ReturnType.ToString(), "");


            modelType.Name = syntaxNode.Identifier.ToString();
            modelType.ReturnValue = new ReturnValueModel
            {
                Type = returnType,
                Modifier = returnTypeModifier
            };
            modelType.ContainingTypeName = containingClassName;
            modelType.Modifier = modifier;
            modelType.AccessModifier = accessModifier;
            modelType.CyclomaticComplexity = CSharpHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

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
    }
}
