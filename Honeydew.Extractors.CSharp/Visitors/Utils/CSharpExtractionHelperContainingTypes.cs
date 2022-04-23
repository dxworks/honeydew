using Honeydew.Extractors.CSharp.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Utils;

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

    public static string GetDefinitionClassName(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        while (true)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(syntaxNode);
            if (symbolInfo.Symbol != null)
            {
                if (symbolInfo.Symbol.ContainingType is null)
                {
                    return symbolInfo.Symbol.ToString();
                }

                return symbolInfo.Symbol.ContainingType.ToString();
            }

            switch (syntaxNode)
            {
                case InvocationExpressionSyntax invocationExpressionSyntax:
                {
                    if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax
                        memberAccessExpressionSyntax)
                    {
                        switch (semanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Expression).Symbol)
                        {
                            case IFieldSymbol fieldSymbol:
                            {
                                return fieldSymbol.Type.Name;
                            }

                            case ILocalSymbol localSymbol:
                            {
                                return localSymbol.Type.ToString();
                            }

                            case { } symbol:
                            {
                                return symbol.ToString();
                            }

                            case null:
                            {
                                switch (memberAccessExpressionSyntax.Expression)
                                {
                                    case IdentifierNameSyntax identifierNameSyntax:
                                    {
                                        return identifierNameSyntax.Identifier.ToString();
                                    }
                                    case ObjectCreationExpressionSyntax objectCreationExpressionSyntax:
                                    {
                                        return objectCreationExpressionSyntax.Type.ToString();
                                    }
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
                        var initializerSymbolInfo =
                            semanticModel.GetSymbolInfo(constructorDeclarationSyntax.Initializer);
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

                case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                {
                    syntaxNode = memberAccessExpressionSyntax.Expression;
                    continue;
                }

                case IdentifierNameSyntax identifierNameSyntax:
                {
                    return identifierNameSyntax.Identifier.ToString();
                }

                default:
                {
                }
                    break;
            }

            return "";
        }
    }

    public static string GetLocationClassName(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        switch (syntaxNode)
        {
            case InvocationExpressionSyntax invocationExpressionSyntax:
            {
                switch (invocationExpressionSyntax.Expression)
                {
                    case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                    {
                        if (GetLocationClassNameFromMemberAccessExpressionSyntax(memberAccessExpressionSyntax,
                                out var className))
                        {
                            return className;
                        }
                    }
                        break;
                }

                break;
            }

            case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
            {
                if (GetLocationClassNameFromMemberAccessExpressionSyntax(memberAccessExpressionSyntax,
                        out var className))
                {
                    return className;
                }
            }
                break;

            case IdentifierNameSyntax identifierNameSyntax:
            {
                var symbolInfo = semanticModel.GetSymbolInfo(identifierNameSyntax);

                switch (symbolInfo.Symbol)
                {
                    case IMethodSymbol methodSymbol:
                    {
                        if (methodSymbol.ReceiverType != null)
                        {
                            return methodSymbol.ReceiverType.ToString();
                        }
                    }
                        break;

                    default:
                    {
                    }
                        break;
                }
            }
                break;

            default:
            {
            }
                break;
        }

        return GetDefinitionClassName(syntaxNode, semanticModel);

        bool GetLocationClassNameFromMemberAccessExpressionSyntax(
            MemberAccessExpressionSyntax memberAccessExpressionSyntax, out string className)
        {
            className = "";
            var symbolInfo = semanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Expression);
            switch (symbolInfo.Symbol)
            {
                case ILocalSymbol localSymbol:
                {
                    className = localSymbol.Type.ToString();
                    return true;
                }

                case IFieldSymbol fieldSymbol:
                {
                    className = fieldSymbol.Type.ToString();
                    return true;
                }

                case IPropertySymbol propertySymbol:
                {
                    className = propertySymbol.Type.ToString();
                    return true;
                }

                case IMethodSymbol:
                {
                    className = GetLocationClassName(memberAccessExpressionSyntax.Name, semanticModel);
                    return true;
                }

                default:
                {
                }
                    break;
            }

            return false;
        }
    }
}
