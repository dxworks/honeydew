using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class ConstructorInfoVisitor : IExtractionVisitor<ConstructorDeclarationSyntax, SemanticModel, IConstructorType>
{
    public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        GetModifiersForNode(syntaxNode, out var accessModifier, out var modifier);

        if (modifier == "static")
        {
            accessModifier = "";
        }

        modelType.Name = syntaxNode.Identifier.ToString();
        modelType.Modifier = modifier;
        modelType.AccessModifier = accessModifier;
        modelType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);

        return modelType;
    }

    private static void GetModifiersForNode(MemberDeclarationSyntax node, out string accessModifier,
        out string modifier)
    {
        var allModifiers = node.Modifiers.ToString();

        accessModifier = CSharpConstants.DefaultClassMethodAccessModifier;
        modifier = allModifiers;

        CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);
    }
}
