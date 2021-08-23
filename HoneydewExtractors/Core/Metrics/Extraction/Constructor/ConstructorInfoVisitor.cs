using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Constructor
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
                containingClassName = CSharpHelperMethods.GetFullName(baseTypeDeclarationSyntax);
            }

            GetModifiersForNode(syntaxNode, out var accessModifier, out var modifier);

            modelType.Name = syntaxNode.Identifier.ToString();
            modelType.ContainingTypeName = containingClassName;
            modelType.Modifier = modifier;
            modelType.AccessModifier = accessModifier;
            modelType.CyclomaticComplexity = CSharpHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

            foreach (var parameterType in CSharpHelperMethods.ExtractInfoAboutParameters(syntaxNode.ParameterList))
            {
                modelType.ParameterTypes.Add(parameterType);
            }

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
