using HoneydewCore.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction;

public static partial class CSharpExtractionHelperMethods
{
    public static string GetContainingNamespaceName(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        var declaredSymbol = semanticModel.GetDeclaredSymbol(syntaxNode);

        if (declaredSymbol != null)
        {
            return declaredSymbol.ContainingNamespace.ToString();
        }

        var namespaceDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BaseNamespaceDeclarationSyntax>();
        if (namespaceDeclarationSyntax != null)
        {
            return namespaceDeclarationSyntax.Name.ToString();
        }

        return "";
    }

    public static string GetContainingClassName(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        var parentDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BaseTypeDeclarationSyntax>();
        if (parentDeclarationSyntax != null)
        {
            return GetFullName(parentDeclarationSyntax, semanticModel).Name;
        }

        return "";
    }

    public static string GetContainingMethodName(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        return "";
    }

    public static string GetOriginalMethodDefinitionClassName(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(syntaxNode);
        if (symbolInfo.Symbol != null)
        {
            return symbolInfo.Symbol.ContainingType.ToString();
        }

        switch (syntaxNode)
        {
            case InvocationExpressionSyntax invocationExpressionSyntax:
            {
                if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
                {
                    switch (semanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Expression).Symbol)
                    {
                        case IFieldSymbol fieldSymbol:
                        {
                            return fieldSymbol.Type.Name;
                        }

                        case { } symbol:
                        {
                            return symbol.ToString();
                        }

                        case null:
                        {
                            if (memberAccessExpressionSyntax.Expression is IdentifierNameSyntax identifierNameSyntax)
                            {
                                return identifierNameSyntax.Identifier.ToString();
                            }
                        }
                            break;
                    }
                }
            }
                break;

            case ConstructorDeclarationSyntax constructorDeclarationSyntax:
            {
                if (constructorDeclarationSyntax.Initializer != null)
                {
                    var initializerSymbolInfo = semanticModel.GetSymbolInfo(constructorDeclarationSyntax.Initializer);
                    if (initializerSymbolInfo.Symbol != null)
                    {
                        return initializerSymbolInfo.Symbol.ContainingType.ToString();
                    }

                    if (constructorDeclarationSyntax.Initializer.ThisOrBaseKeyword.Text ==
                        CSharpConstants.BaseClassIdentifier)
                    {
                        var baseTypeDeclarationSyntax =
                            syntaxNode.GetParentDeclarationSyntax<BaseTypeDeclarationSyntax>();
                        if (baseTypeDeclarationSyntax is { BaseList.Types.Count: > 0 })
                        {
                            return baseTypeDeclarationSyntax.BaseList.Types[0].Type.ToString();
                        }
                    }
                }
            }
                break;
        }

        return "";
    }

    public static string GetActualMethodDefinitionClassName(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        return GetOriginalMethodDefinitionClassName(syntaxNode, semanticModel);
    }
}
