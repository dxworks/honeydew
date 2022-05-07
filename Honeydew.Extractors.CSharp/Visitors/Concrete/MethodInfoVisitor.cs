using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Models.CSharp.CSharpConstants;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class MethodInfoVisitor :
    IExtractionVisitor<MethodDeclarationSyntax, SemanticModel, IMethodType>,
    IExtractionVisitor<AccessorDeclarationSyntax, SemanticModel, IAccessorMethodType>,
    IExtractionVisitor<ArrowExpressionClauseSyntax, SemanticModel, IAccessorMethodType>
{
    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        var isInterface = syntaxNode.GetParentDeclarationSyntax<InterfaceDeclarationSyntax>() != null;
        var accessModifier = isInterface
            ? DefaultInterfaceMethodAccessModifier
            : DefaultClassMethodAccessModifier;
        var modifier = isInterface
            ? DefaultInterfaceMethodModifier
            : syntaxNode.Modifiers.ToString();

        SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier, ref modifier);

        modelType.Name = syntaxNode.Identifier.ToString();

        modelType.Modifier = modifier;
        modelType.AccessModifier = accessModifier;
        modelType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);

        return modelType;
    }

    public IAccessorMethodType Visit(AccessorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorMethodType modelMethodType)
    {
        var accessModifier = "public";
        var modifier = syntaxNode.Modifiers.ToString();

        SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier, ref modifier);

        modelMethodType.Name = syntaxNode.Keyword.ToString();

        modelMethodType.Modifier = modifier;
        modelMethodType.AccessModifier = accessModifier;
        modelMethodType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);

        return modelMethodType;
    }

    public IAccessorMethodType Visit(ArrowExpressionClauseSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorMethodType modelMethodType)
    {
        modelMethodType.Name = "get";
        modelMethodType.AccessModifier = "public";

        modelMethodType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);

        return modelMethodType;
    }
}
