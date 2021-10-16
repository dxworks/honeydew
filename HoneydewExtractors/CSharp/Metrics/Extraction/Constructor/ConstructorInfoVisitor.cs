using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Constructor
{
    public class ConstructorInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpConstructorVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, IConstructorType modelType)
        {
            var containingClassName = "";
            if (syntaxNode.Parent is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
            {
                containingClassName = CSharpHelperMethods.GetFullName(baseTypeDeclarationSyntax).Name;
            }

            GetModifiersForNode(syntaxNode, out var accessModifier, out var modifier);

            if (modifier == "static")
            {
                accessModifier = "";
            }
            
            modelType.Name = syntaxNode.Identifier.ToString();
            modelType.ContainingTypeName = containingClassName;
            modelType.Modifier = modifier;
            modelType.AccessModifier = accessModifier;
            modelType.CyclomaticComplexity = CSharpHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

            return modelType;
        }

        private void GetModifiersForNode(MemberDeclarationSyntax node, out string accessModifier, out string modifier)
        {
            var allModifiers = node.Modifiers.ToString();

            accessModifier = CSharpConstants.DefaultClassMethodAccessModifier;
            modifier = allModifiers;

            CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);
        }
    }
}
