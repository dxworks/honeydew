using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicFullNameProvider;

namespace Honeydew.Extractors.VisualBasic.Visitors.Utils;

public static partial class VisualBasicExtractionHelperMethods
{
    public static IEntityType GetFullName(SyntaxNode syntaxNode, SemanticModel semanticModel, out bool isNullable)
    {
        return VisualBasicFullNameProvider.GetFullName(syntaxNode, semanticModel, out isNullable);
    }

    public static IEntityType GetFullName(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        return VisualBasicFullNameProvider.GetFullName(syntaxNode, semanticModel, out _);
    }

    public static VisualBasicMethodCallModel GetMethodCallModel(InvocationExpressionSyntax invocationExpressionSyntax,
        SemanticModel semanticModel)
    {
        var methodName = invocationExpressionSyntax.Expression switch
        {
            MemberAccessExpressionSyntax memberAccessExpressionSyntax => memberAccessExpressionSyntax.Name.ToString(),
            _ => invocationExpressionSyntax.Expression.ToString()
        };

        return new VisualBasicMethodCallModel
        {
            Name = methodName,
            DefinitionClassName = GetDefinitionClassName(invocationExpressionSyntax, semanticModel),
            LocationClassName = GetLocationClassName(invocationExpressionSyntax, semanticModel),
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
                .Select(parameter => VisualBasicFullTypeNameBuilder.CreateEntityTypeModel(parameter.Name))
                .Cast<IEntityType>()
                .ToList();
        }

        return new List<IEntityType>();
    }

    private static IMethodSymbol? GetMethodSymbol(SyntaxNode expressionSyntax, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(expressionSyntax);
        var symbol = symbolInfo.Symbol;
        if (symbol is IMethodSymbol methodSymbol)
        {
            return methodSymbol;
        }

        return null;
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

    public static IList<IParameterType> GetParameters(MemberAccessExpressionSyntax methodCall,
        SemanticModel semanticModel)
    {
        IList<IParameterType> parameters = new List<IParameterType>();

        var symbolInfo = semanticModel.GetSymbolInfo(methodCall);
        if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
        {
            foreach (var parameter in methodSymbol.Parameters)
            {
                // var modifier = parameterSyntax.Modifiers.ToString();
                // todo parameter modifiers
                var modifier = "";
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

                parameters.Add(new VisualBasicParameterModel
                {
                    Type = CreateEntityTypeModel(parameter.Type.ToString()),
                    Modifier = modifier,
                    DefaultValue = defaultValue
                });
            }
        }

        return parameters;
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
                var argumentExpressionSyntax = argumentSyntax.GetExpression();
                var parameterSymbolInfo = semanticModel.GetSymbolInfo(argumentExpressionSyntax);
                if (parameterSymbolInfo.Symbol != null)
                {
                    parameterList.Add(new VisualBasicParameterModel
                    {
                        Type = GetFullName(argumentExpressionSyntax, semanticModel)
                    });
                    continue;
                }

                if (argumentExpressionSyntax is not LiteralExpressionSyntax literalExpressionSyntax)
                {
                    success = false;
                    break;
                }

                if (literalExpressionSyntax.Token.Value == null)
                {
                    success = false;
                    break;
                }

                parameterList.Add(new VisualBasicParameterModel
                {
                    Type = CreateEntityTypeModel(literalExpressionSyntax.Token.Value.GetType()
                        .FullName),
                });
            }

            return success ? parameterList : new List<IParameterType>();
        }

        return GetParameters(methodSymbol);
    }


    private static IList<IParameterType> GetParameters(IMethodSymbol methodSymbol)
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

            parameters.Add(new VisualBasicParameterModel
            {
                Type = CreateEntityTypeModel(parameter.Type.ToString()),
                Modifier = modifier,
                DefaultValue = defaultValue
            });
        }

        return parameters;
    }

    public static int CalculateCyclomaticComplexity(MethodBlockSyntax methodBlockSyntax)
    {
        return CalculateCyclomaticComplexityForSyntaxNode(methodBlockSyntax) + 1;
    }

    public static int CalculateCyclomaticComplexity(MethodStatementSyntax methodStatementSyntax)
    {
        if (methodStatementSyntax.Parent is MethodBlockSyntax methodBlockSyntax)
        {
            return CalculateCyclomaticComplexity(methodBlockSyntax);
        }

        return CalculateCyclomaticComplexityForSyntaxNode(methodStatementSyntax) + 1;
    }

    public static int CalculateCyclomaticComplexity(ConstructorBlockSyntax constructorBlockSyntax)
    {
        return CalculateCyclomaticComplexityForSyntaxNode(constructorBlockSyntax) + 1;
    }

    public static int CalculateCyclomaticComplexity(PropertyStatementSyntax propertyBlockSyntax)
    {
        return CalculateCyclomaticComplexityForSyntaxNode(propertyBlockSyntax) + 1;
    }

    public static int CalculateCyclomaticComplexity(AccessorBlockSyntax syntax)
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
                return syntaxKind is SyntaxKind.AndKeyword or SyntaxKind.OrKeyword;
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
                case ElseIfBlockSyntax elseIfBlockSyntax:
                {
                    count += CalculateCyclomaticComplexity(elseIfBlockSyntax.ElseIfStatement.Condition);
                }
                    break;
                case ElseIfStatementSyntax elseIfStatementSyntax:
                {
                    count += CalculateCyclomaticComplexity(elseIfStatementSyntax.Condition);
                }
                    break;

                case ForStatementSyntax:
                case DoStatementSyntax:
                case CaseStatementSyntax:
                case ElseCaseClauseSyntax:
                case ForEachStatementSyntax:
                case ConditionalAccessExpressionSyntax:
                    count++;
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
                var expressionSyntax = argumentSyntax.GetExpression();
                var parameterSymbolInfo = semanticModel.GetSymbolInfo(expressionSyntax);
                var parameterType = "System.Object";

                if (parameterSymbolInfo.Symbol != null)
                {
                    parameterTypes.Add(new VisualBasicParameterModel
                    {
                        Type = GetFullName(expressionSyntax, semanticModel)
                    });
                    continue;
                }

                if (expressionSyntax is LiteralExpressionSyntax literalExpressionSyntax)
                {
                    if (literalExpressionSyntax.Token.Value != null)
                    {
                        parameterType = literalExpressionSyntax.Token.Value.GetType().FullName;
                    }
                }

                parameterTypes.Add(new VisualBasicParameterModel
                {
                    Type = CreateEntityTypeModel(parameterType)
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
        if (syntaxNode.Target == null)
        {
            return syntaxNode.Parent?.Parent switch
            {
                MethodStatementSyntax => "method",
                MethodBlockSyntax => "method",
                ConstructorBlockSyntax => "method",
                SubNewStatementSyntax => "method",
                AccessorStatementSyntax => "method",
                TypeBlockSyntax => "type",
                TypeStatementSyntax => "type",
                DelegateStatementSyntax => "type",
                EnumBlockSyntax => "type",
                EnumStatementSyntax => "type",
                EnumMemberDeclarationSyntax => "field",
                FieldDeclarationSyntax => "field",
                PropertyBlockSyntax => "property",
                PropertyStatementSyntax => "property",
                ParameterSyntax => "param",
                TypeParameterSyntax => "param",
                TypeSyntax => "return",
                _ => ""
            };
        }

        return syntaxNode.Target.ToString().TrimEnd(':');
    }
}
