using System.Text;
using Honeydew.Extractors.Dotnet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Utils;

public static partial class VisualBasicExtractionHelperMethods
{
    public static string GetContainingNamespaceName(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        var declaredSymbol = semanticModel.GetDeclaredSymbol(syntaxNode);

        if (declaredSymbol != null)
        {
            return declaredSymbol.ContainingNamespace.ToString() ?? "";
        }

        var namespaceDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<NamespaceStatementSyntax>();
        return namespaceDeclarationSyntax != null ? namespaceDeclarationSyntax.Name.ToString() : "";
    }

    public static string GetContainingModuleName(SyntaxNode syntaxNode)
    {
        var moduleBlockSyntax = syntaxNode.GetParentDeclarationSyntax<ModuleBlockSyntax>();
        var lastModuleBlockSyntax = moduleBlockSyntax;
        var moduleBlockNamesStack = new Stack<string>();

        while (moduleBlockSyntax is not null)
        {
            moduleBlockNamesStack.Push(moduleBlockSyntax.BlockStatement.Identifier.ToString());

            lastModuleBlockSyntax = moduleBlockSyntax;
            moduleBlockSyntax = moduleBlockSyntax.GetParentDeclarationSyntax<ModuleBlockSyntax>();
        }

        var namespaceBlockSyntax = lastModuleBlockSyntax?.GetParentDeclarationSyntax<NamespaceBlockSyntax>();
        while (namespaceBlockSyntax is not null)
        {
            moduleBlockNamesStack.Push(namespaceBlockSyntax.NamespaceStatement.Name.ToString());

            namespaceBlockSyntax = namespaceBlockSyntax.GetParentDeclarationSyntax<NamespaceBlockSyntax>();
        }

        var stringBuilder = new StringBuilder();
        while (moduleBlockNamesStack.Count > 0)
        {
            var name = moduleBlockNamesStack.Pop();
            stringBuilder.Append(name);
            if (moduleBlockNamesStack.Count > 0)
            {
                stringBuilder.Append('.');
            }
        }

        return stringBuilder.ToString();
    }

    public static string GetContainingClassName(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        var parentTypeBlockSyntax = syntaxNode.GetParentDeclarationSyntax<TypeBlockSyntax>();

        if (parentTypeBlockSyntax is ModuleBlockSyntax)
        {
            return "";
        }

        return parentTypeBlockSyntax is not null ? GetFullName(parentTypeBlockSyntax, semanticModel).Name : "";
    }

    public static string GetDefinitionClassName(SyntaxNode? syntaxNode, SemanticModel semanticModel)
    {
        // todo ask someone about trimming ?
        while (true)
        {
            if (syntaxNode is null)
            {
                return "";
            }

            var symbolInfo = semanticModel.GetSymbolInfo(syntaxNode);
            if (symbolInfo.Symbol != null)
            {
                if (symbolInfo.Symbol.ContainingType is null)
                {
                    return symbolInfo.Symbol.ToString()?.TrimEnd('?') ?? "";
                }

                return symbolInfo.Symbol.ContainingType.ToString()?.TrimEnd('?') ?? "";
            }

            switch (syntaxNode)
            {
                case InvocationExpressionSyntax invocationExpressionSyntax:
                {
                    if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax
                            memberAccessExpressionSyntax &&
                        memberAccessExpressionSyntax.Expression is not null)
                    {
                        switch (semanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Expression).Symbol)
                        {
                            case IFieldSymbol fieldSymbol:
                            {
                                return fieldSymbol.Type.Name.TrimEnd('?');
                            }

                            case ILocalSymbol localSymbol:
                            {
                                return localSymbol.Type.ToString()?.TrimEnd('?') ?? "";
                            }

                            case { } symbol:
                            {
                                return symbol.ToString()?.TrimEnd('?') ?? "";
                            }

                            case null:
                            {
                                switch (memberAccessExpressionSyntax.Expression)
                                {
                                    case IdentifierNameSyntax identifierNameSyntax:
                                    {
                                        return identifierNameSyntax.Identifier.ToString().TrimEnd('?');
                                    }
                                    case ObjectCreationExpressionSyntax objectCreationExpressionSyntax:
                                    {
                                        return objectCreationExpressionSyntax.Type.ToString().TrimEnd('?');
                                    }
                                }
                            }
                                break;
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
                    return identifierNameSyntax.Identifier.ToString().TrimEnd('?');
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
                            return className?.Trim('?') ?? "";
                        }
                    }
                        break;
                }

                break;
            }

            case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
            {
                if (memberAccessExpressionSyntax.Expression is MyBaseExpressionSyntax)
                {
                    var typeBlockSyntax = memberAccessExpressionSyntax.GetParentDeclarationSyntax<TypeBlockSyntax>();
                    if (typeBlockSyntax is not null)
                    {
                        var inheritsStatementSyntax = typeBlockSyntax.Inherits.FirstOrDefault();

                        var inheritedType = inheritsStatementSyntax?.Types.FirstOrDefault();
                        if (inheritedType is null)
                        {
                            return "Object";
                        }

                        var symbol = semanticModel.GetSymbolInfo(inheritedType).Symbol;
                        if (symbol is null)
                        {
                            return inheritedType.ToString().Trim('?');
                        }

                        return symbol.ToString()?.Trim('?') ?? "";
                    }
                }

                if (GetLocationClassNameFromMemberAccessExpressionSyntax(memberAccessExpressionSyntax,
                        out var className))
                {
                    return className?.TrimEnd('?') ?? "";
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
                            var locationClassName = methodSymbol.ReceiverType.ToString() ?? "";
                            return locationClassName.TrimEnd('?');
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

        return GetDefinitionClassName(syntaxNode, semanticModel).TrimEnd('?');

        bool GetLocationClassNameFromMemberAccessExpressionSyntax(
            MemberAccessExpressionSyntax memberAccessExpressionSyntax, out string? className)
        {
            className = "";
            if (memberAccessExpressionSyntax.Expression is null)
            {
                return false;
            }

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

                case IParameterSymbol parameterSymbol:
                {
                    if (parameterSymbol.IsThis)
                    {
                        className = parameterSymbol.Type.ToString();
                        return true;
                    }
                }
                    break;

                default:
                {
                }
                    break;
            }

            return false;
        }
    }
}
