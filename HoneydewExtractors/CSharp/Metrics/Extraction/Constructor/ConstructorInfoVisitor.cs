using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;

public class ConstructorInfoVisitor : ICSharpConstructorVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, SemanticModel semanticModel, IConstructorType modelType)
    {
        var containingClassName = "";
        if (syntaxNode.Parent is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
        {
            containingClassName = CSharpExtractionHelperMethods.GetFullName(baseTypeDeclarationSyntax, semanticModel).Name;
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
        modelType.CyclomaticComplexity = CSharpExtractionHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

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
