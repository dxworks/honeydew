using System.Text;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Utils;

public static partial class CSharpExtractionHelperMethods
{
    public static IEntityType GetFullName(SyntaxNode syntaxNode, SemanticModel semanticModel, out bool isNullable)
    {
        return CSharpFullNameProvider.GetFullName(syntaxNode, semanticModel, out isNullable);
    }

    public static IEntityType GetFullName(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        return CSharpFullNameProvider.GetFullName(syntaxNode, semanticModel, out _);
    }

    public static IEnumerable<IEntityType> GetBaseInterfaces(BaseTypeDeclarationSyntax node,
        SemanticModel semanticModel)
    {
        var declaredSymbol = semanticModel.GetDeclaredSymbol(node);

        IList<IEntityType> interfaces = new List<IEntityType>();

        if (declaredSymbol is not ITypeSymbol typeSymbol)
        {
            return interfaces;
        }

        foreach (var interfaceSymbol in typeSymbol.Interfaces)
        {
            interfaces.Add(CSharpFullNameProvider.CreateEntityTypeModel(interfaceSymbol.ToString()));
        }

        return interfaces;
    }

    private static IEntityType GetBaseClassName(TypeDeclarationSyntax node, SemanticModel semanticModel)
    {
        var declaredSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, node);

        if (declaredSymbol is not ITypeSymbol typeSymbol)
        {
            return CSharpFullNameProvider.CreateEntityTypeModel(CSharpConstants.ObjectIdentifier);
        }

        if (typeSymbol.BaseType == null)
        {
            return CSharpFullNameProvider.CreateEntityTypeModel(CSharpConstants.ObjectIdentifier);
        }

        return CSharpFullNameProvider.CreateEntityTypeModel(typeSymbol.BaseType.ToString());
    }

    public static IEntityType GetBaseClassName(BaseTypeDeclarationSyntax baseTypeDeclarationSyntax,
        SemanticModel semanticModel)
    {
        if (baseTypeDeclarationSyntax is TypeDeclarationSyntax typeDeclarationSyntax)
        {
            return GetBaseClassName(typeDeclarationSyntax, semanticModel);
        }

        return CSharpFullNameProvider.CreateEntityTypeModel(CSharpConstants.ObjectIdentifier);
    }

    public static IMethodSymbol? GetMethodSymbol(CSharpSyntaxNode expressionSyntax, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(expressionSyntax);
        var symbol = symbolInfo.Symbol;
        if (symbol is IMethodSymbol methodSymbol)
        {
            return methodSymbol;
        }

        return null;
    }

    public static CSharpMethodCallModel GetMethodCallModel(InvocationExpressionSyntax invocationExpressionSyntax,
        SemanticModel semanticModel)
    {
        var methodName = invocationExpressionSyntax.Expression switch
        {
            MemberAccessExpressionSyntax memberAccessExpressionSyntax => memberAccessExpressionSyntax.Name.ToString(),
            _ => invocationExpressionSyntax.Expression.ToString()
        };

        return new CSharpMethodCallModel
        {
            Name = methodName,
            DefinitionClassName =
                GetDefinitionClassName(invocationExpressionSyntax, semanticModel),
            LocationClassName =
                GetLocationClassName(invocationExpressionSyntax, semanticModel),
            ParameterTypes = GetParameters(invocationExpressionSyntax, semanticModel),
            MethodDefinitionNames = GetMethodDefinitionNames(invocationExpressionSyntax, semanticModel),
            GenericParameters = GetGenericParameters(invocationExpressionSyntax, semanticModel),
        };
    }

    private static IList<IEntityType> GetGenericParameters(InvocationExpressionSyntax invocationExpressionSyntax,
        SemanticModel semanticModel)
    {
        var methodSymbol = GetMethodSymbol(invocationExpressionSyntax, semanticModel);
        if (methodSymbol != null)
        {
            return methodSymbol.TypeParameters
                .Select(parameter => CSharpFullTypeNameBuilder.CreateEntityTypeModel(parameter.Name))
                .Cast<IEntityType>()
                .ToList();
        }

        return new List<IEntityType>();
    }

    private static IList<string> GetMethodDefinitionNames(ISymbol symbol)
    {
        var definitionNames = new List<string>();

        var currentSymbol = symbol.ContainingSymbol;

        while (currentSymbol is IMethodSymbol methodSymbol)
        {
            var containingSymbol =
                methodSymbol.ContainingSymbol == null ? "" : methodSymbol.ContainingSymbol.ToString() ?? "";
            var containingNamespace = methodSymbol.ContainingNamespace == null
                ? ""
                : methodSymbol.ContainingNamespace.ToString() ?? "";
            var symbolName = containingSymbol.Replace(containingNamespace, "").Trim('.');
            var methodSymbolName = methodSymbol.MethodKind switch
            {
                MethodKind.Constructor => symbolName,
                MethodKind.Destructor => $"~{symbolName}",
                _ => methodSymbol.Name
            };

            if (methodSymbol.AssociatedSymbol != null)
            {
                var associatedSymbolName = methodSymbol.AssociatedSymbol.Name;
                var index = methodSymbolName.IndexOf($"_{associatedSymbolName}", StringComparison.Ordinal);
                var methodName = methodSymbolName;
                if (index >= 0)
                {
                    methodName = methodName.Remove(index);
                }

                definitionNames.Add(methodName);
                definitionNames.Add(associatedSymbolName);
            }
            else
            {
                var signature =
                    $"{methodSymbolName}({string.Join(", ", methodSymbol.Parameters.Select(p => p.Type.ToString()))})";

                definitionNames.Add(signature);
            }

            currentSymbol = currentSymbol.ContainingSymbol;
        }

        definitionNames.Reverse();

        return definitionNames;
    }

    private static IList<string> GetMethodDefinitionNames(InvocationExpressionSyntax invocationExpressionSyntax,
        SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpressionSyntax);
        if (symbolInfo.Symbol != null)
        {
            return GetMethodDefinitionNames(symbolInfo.Symbol);
        }

        return new List<string>();
    }

    public static IList<IParameterType> GetParameters(IMethodSymbol methodSymbol)
    {
        IList<IParameterType> parameters = new List<IParameterType>();
        foreach (var parameter in methodSymbol.Parameters)
        {
            var modifier = parameter.RefKind switch
            {
                RefKind.In => "in",
                RefKind.Out => "out",
                RefKind.Ref => "ref",
                _ => ""
            };

            if (parameter.IsParams)
            {
                modifier = "params";
            }

            if (parameter.IsThis)
            {
                modifier = "this";
            }

            string? defaultValue = null;
            if (parameter.HasExplicitDefaultValue)
            {
                defaultValue = parameter.ExplicitDefaultValue == null
                    ? "null"
                    : parameter.ExplicitDefaultValue.ToString();
                if (defaultValue is "False" or "True")
                {
                    defaultValue = defaultValue.ToLower();
                }
            }

            parameters.Add(new CSharpParameterModel
            {
                Type = CSharpFullNameProvider.CreateEntityTypeModel(parameter.Type.ToString()),
                Modifier = modifier,
                DefaultValue = defaultValue
            });
        }

        return parameters;
    }

    public static string GetAliasTypeOfNamespace(NameSyntax nodeName, SemanticModel semanticModel)
    {
        var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, nodeName);
        return symbolInfo.Symbol switch
        {
            null => nameof(EAliasType.NotDetermined),
            INamespaceSymbol => nameof(EAliasType.Namespace),
            _ => nameof(EAliasType.Class)
        };
    }

    public static CSharpParameterModel? ExtractInfoAboutParameter(BaseParameterSyntax baseParameterSyntax,
        SemanticModel semanticModel)
    {
        if (baseParameterSyntax.Type is null)
        {
            return null;
        }
        
        var parameterType = GetFullName(baseParameterSyntax.Type, semanticModel, out var isNullable);

        string? defaultValue = null;

        if (baseParameterSyntax is ParameterSyntax parameterSyntax)
        {
            defaultValue = parameterSyntax.Default?.Value.ToString();
        }

        return new CSharpParameterModel
        {
            Type = parameterType,
            Modifier = baseParameterSyntax.Modifiers.ToString(),
            DefaultValue = defaultValue,
            IsNullable = isNullable
        };
    }

    public static IEntityType GetContainingType(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        if (syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax)
        {
            switch (invocationExpressionSyntax.Expression)
            {
                case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                {
                    var entityType = GetFullName(memberAccessExpressionSyntax.Expression, semanticModel);
                    if (string.IsNullOrEmpty(entityType.Name))
                    {
                        entityType = GetFullName(memberAccessExpressionSyntax.Name, semanticModel);
                    }

                    return entityType;
                }

                default:
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(syntaxNode);
                    if (symbolInfo.Symbol != null)
                    {
                        var containingSymbol = symbolInfo.Symbol.ContainingSymbol;
                        var stringBuilder = new StringBuilder(containingSymbol.ToDisplayString());
                        while (containingSymbol.Kind == SymbolKind.Method)
                        {
                            containingSymbol = containingSymbol.ContainingSymbol;
                            if (containingSymbol.Kind == SymbolKind.Method)
                            {
                                stringBuilder = new StringBuilder(containingSymbol.ToDisplayString())
                                    .Append('.')
                                    .Append(stringBuilder);
                            }
                        }

                        return CSharpFullNameProvider.CreateEntityTypeModel(stringBuilder.ToString());
                    }

                    var typeInfo = semanticModel.GetTypeInfo(syntaxNode);
                    if (typeInfo.Type != null)
                    {
                        return CSharpFullNameProvider.CreateEntityTypeModel(typeInfo.Type.ToDisplayString());
                    }
                }
                    break;
            }
        }

        var baseFieldDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BaseFieldDeclarationSyntax>();
        if (baseFieldDeclarationSyntax != null)
        {
            return GetFullName(baseFieldDeclarationSyntax.Declaration.Type, semanticModel);
        }

        var basePropertyDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
        if (basePropertyDeclarationSyntax != null)
        {
            return GetFullName(basePropertyDeclarationSyntax.Type, semanticModel);
        }

        var variableDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<VariableDeclarationSyntax>();
        if (variableDeclarationSyntax != null)
        {
            var fullName = GetFullName(variableDeclarationSyntax, semanticModel);
            if (fullName.Name != CSharpConstants.VarIdentifier)
            {
                return fullName;
            }
        }

        return CSharpFullNameProvider.CreateEntityTypeModel(syntaxNode.ToString());
    }

    private static IList<IParameterType> GetParameters(InvocationExpressionSyntax invocationSyntax,
        SemanticModel semanticModel)
    {
        var methodSymbol = GetMethodSymbol(invocationSyntax, semanticModel);
        if (methodSymbol == null)
        {
            // try to reconstruct the parameters from the method call
            var parameterList = new List<IParameterType>();
            var success = true;

            foreach (var argumentSyntax in invocationSyntax.ArgumentList.Arguments)
            {
                var parameterSymbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, argumentSyntax.Expression);
                if (parameterSymbolInfo.Symbol != null)
                {
                    parameterList.Add(new CSharpParameterModel
                    {
                        Type = GetFullName(argumentSyntax.Expression, semanticModel)
                    });
                    continue;
                }

                if (argumentSyntax.Expression is not LiteralExpressionSyntax literalExpressionSyntax)
                {
                    success = false;
                    break;
                }

                if (literalExpressionSyntax.Token.Value == null)
                {
                    success = false;
                    break;
                }

                parameterList.Add(new CSharpParameterModel
                {
                    Type = CSharpFullNameProvider.CreateEntityTypeModel(literalExpressionSyntax.Token.Value.GetType()
                        .FullName),
                });
            }

            return success ? parameterList : new List<IParameterType>();
        }

        return GetParameters(methodSymbol);
    }

    public static string SetTypeModifier(string typeString, string modifier)
    {
        if (typeString.StartsWith(CSharpConstants.RefReadonlyIdentifier))
        {
            modifier += $" {CSharpConstants.RefReadonlyIdentifier}";
            modifier = modifier.Trim();
        }
        else if (typeString.StartsWith(CSharpConstants.RefIdentifier))
        {
            modifier += $" {CSharpConstants.RefIdentifier}";
            modifier = modifier.Trim();
        }

        return modifier;
    }

    public static int CalculateCyclomaticComplexity(ArrowExpressionClauseSyntax syntax)
    {
        return CalculateCyclomaticComplexityForSyntaxNode(syntax) + 1;
    }

    public static int CalculateCyclomaticComplexity(AccessorDeclarationSyntax syntax)
    {
        return CalculateCyclomaticComplexityForSyntaxNode(syntax) + 1;
    }

    public static int CalculateCyclomaticComplexity(MemberDeclarationSyntax syntax)
    {
        if (syntax is not BasePropertyDeclarationSyntax basePropertyDeclarationSyntax)
        {
            return CalculateCyclomaticComplexityForSyntaxNode(syntax) + 1;
        }

        var accessorCount = 0;
        if (basePropertyDeclarationSyntax.AccessorList != null)
        {
            accessorCount = basePropertyDeclarationSyntax.AccessorList.Accessors.Count;
        }

        return CalculateCyclomaticComplexityForSyntaxNode(syntax) + accessorCount;
    }

    public static int CalculateCyclomaticComplexity(LocalFunctionStatementSyntax syntax)
    {
        return CalculateCyclomaticComplexityForSyntaxNode(syntax) + 1;
    }

    private static int CalculateCyclomaticComplexity(ExpressionSyntax? conditionExpressionSyntax)
    {
        if (conditionExpressionSyntax == null)
        {
            return 1;
        }

        var logicalOperatorsCount = conditionExpressionSyntax
            .DescendantTokens()
            .Count(token =>
            {
                var syntaxKind = token.Kind();
                return syntaxKind is SyntaxKind.AmpersandAmpersandToken or SyntaxKind.BarBarToken or SyntaxKind
                    .AndKeyword or SyntaxKind.OrKeyword;
            });

        return logicalOperatorsCount + 1;
    }

    private static int CalculateCyclomaticComplexityForSyntaxNode(SyntaxNode syntax)
    {
        var count = 0;
        foreach (var descendantNode in syntax.DescendantNodes())
        {
            switch (descendantNode)
            {
                case WhileStatementSyntax whileStatementSyntax:
                {
                    count += CalculateCyclomaticComplexity(whileStatementSyntax.Condition);
                }
                    break;
                case IfStatementSyntax ifStatementSyntax:
                {
                    count += CalculateCyclomaticComplexity(ifStatementSyntax.Condition);
                }
                    break;
                case ForStatementSyntax forStatementSyntax:
                {
                    count += CalculateCyclomaticComplexity(forStatementSyntax.Condition);
                }
                    break;
                case DoStatementSyntax:
                case CaseSwitchLabelSyntax:
                case DefaultSwitchLabelSyntax:
                case CasePatternSwitchLabelSyntax:
                case ForEachStatementSyntax:
                case ConditionalExpressionSyntax:
                case ConditionalAccessExpressionSyntax:
                    count++;
                    break;
                default:
                {
                    switch (descendantNode.Kind())
                    {
                        case SyntaxKind.CoalesceAssignmentExpression:
                        case SyntaxKind.CoalesceExpression:
                            count++;
                            break;
                    }
                }
                    break;
            }
        }

        return count;
    }

    public static IEnumerable<IParameterType> GetParameters(AttributeSyntax attributeSyntax,
        SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(attributeSyntax);
        if (symbolInfo.Symbol == null)
        {
            // try to reconstruct parameters
            var parameterTypes = new List<IParameterType>();

            if (attributeSyntax.ArgumentList == null)
            {
                return parameterTypes;
            }

            foreach (var argumentSyntax in attributeSyntax.ArgumentList.Arguments)
            {
                var parameterSymbolInfo = semanticModel.GetSymbolInfo(argumentSyntax.Expression);
                var parameterType = CSharpConstants.SystemObject;
                if (parameterSymbolInfo.Symbol != null)
                {
                    parameterTypes.Add(new CSharpParameterModel
                    {
                        Type = GetFullName(argumentSyntax.Expression, semanticModel)
                    });
                    continue;
                }

                if (argumentSyntax.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                {
                    if (literalExpressionSyntax.Token.Value != null)
                    {
                        parameterType = literalExpressionSyntax.Token.Value.GetType().FullName;
                    }
                }

                parameterTypes.Add(new CSharpParameterModel
                {
                    Type = CSharpFullNameProvider.CreateEntityTypeModel(parameterType)
                });
            }

            return parameterTypes;
        }

        if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
        {
            return GetParameters(methodSymbol);
        }

        return new List<IParameterType>();
    }

    public static string GetAttributeTarget(AttributeSyntax syntaxNode)
    {
        var attributeListSyntax = syntaxNode.GetParentDeclarationSyntax<AttributeListSyntax>();

        if (attributeListSyntax is null)
        {
            return "";
        }

        if (attributeListSyntax.Target == null)
        {
            return attributeListSyntax.Parent switch
            {
                BaseMethodDeclarationSyntax => "method",
                AccessorDeclarationSyntax => "method",
                ArrowExpressionClauseSyntax => "method",
                TypeDeclarationSyntax => "type",
                DelegateDeclarationSyntax => "type",
                EnumDeclarationSyntax => "type",
                EnumMemberDeclarationSyntax => "field",
                BaseFieldDeclarationSyntax => "field",
                BasePropertyDeclarationSyntax => "property",
                ParameterSyntax => "param",
                TypeParameterSyntax => "param",
                TypeSyntax => "return",
                _ => ""
            };
        }

        return attributeListSyntax.Target.Identifier.ToString().TrimEnd(':');
    }
}
