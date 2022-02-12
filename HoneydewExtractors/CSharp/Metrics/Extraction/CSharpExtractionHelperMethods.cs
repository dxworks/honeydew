﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using HoneydewCore.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction;

public static class CSharpExtractionHelperMethods
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

    public static IMethodSymbol GetMethodSymbol(CSharpSyntaxNode expressionSyntax, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(expressionSyntax);
        var symbol = symbolInfo.Symbol;
        if (symbol is IMethodSymbol methodSymbol)
        {
            return methodSymbol;
        }

        return null;
    }

    public static MethodCallModel GetMethodCallModel(InvocationExpressionSyntax invocationExpressionSyntax,
        SemanticModel semanticModel)
    {
        var calledMethodName = invocationExpressionSyntax.Expression.ToString();

        var containingClassName = GetContainingType(invocationExpressionSyntax, semanticModel).Name;

        return invocationExpressionSyntax.Expression switch
        {
            MemberAccessExpressionSyntax memberAccessExpressionSyntax => new MethodCallModel
            {
                Name = memberAccessExpressionSyntax.Name.ToString(),
                ContainingTypeName = containingClassName,
                ParameterTypes = GetParameters(invocationExpressionSyntax, semanticModel)
            },
            _ => new MethodCallModel
            {
                Name = calledMethodName,
                ContainingTypeName = containingClassName,
                ParameterTypes = GetParameters(invocationExpressionSyntax, semanticModel)
            }
        };
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

            string defaultValue = null;
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

            parameters.Add(new ParameterModel
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

    public static ParameterModel ExtractInfoAboutParameter(BaseParameterSyntax baseParameterSyntax,
        SemanticModel semanticModel)
    {
        var parameterType = GetFullName(baseParameterSyntax.Type, semanticModel, out var isNullable);

        string defaultValue = null;

        if (baseParameterSyntax is ParameterSyntax parameterSyntax)
        {
            defaultValue = parameterSyntax.Default?.Value.ToString();
        }

        return new ParameterModel
        {
            Type = parameterType,
            Modifier = baseParameterSyntax.Modifiers.ToString(),
            DefaultValue = defaultValue,
            IsNullable = isNullable
        };
    }

    public static string GetParentDeclaredType(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        CSharpSyntaxNode declarationSyntax = syntaxNode.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>();

        declarationSyntax ??= syntaxNode.GetParentDeclarationSyntax<BaseMethodDeclarationSyntax>();

        declarationSyntax ??= syntaxNode.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();

        declarationSyntax ??= syntaxNode.GetParentDeclarationSyntax<EventDeclarationSyntax>();

        declarationSyntax ??= syntaxNode.GetParentDeclarationSyntax<DelegateDeclarationSyntax>();

        declarationSyntax ??= syntaxNode.GetParentDeclarationSyntax<BaseTypeDeclarationSyntax>();

        declarationSyntax ??= syntaxNode.GetParentDeclarationSyntax<NamespaceDeclarationSyntax>();

        declarationSyntax ??= syntaxNode.GetParentDeclarationSyntax<FileScopedNamespaceDeclarationSyntax>();

        if (declarationSyntax != null)
        {
            var declaredSymbol = semanticModel.GetDeclaredSymbol(declarationSyntax);
            if (declaredSymbol == null)
            {
                return syntaxNode.ToString();
            }

            if (declarationSyntax is LocalFunctionStatementSyntax)
            {
                var parent = GetParentDeclaredType(declarationSyntax.Parent, semanticModel);
                return $"{parent}.{declaredSymbol}";
            }

            return declaredSymbol.ToString();
        }

        return syntaxNode.ToString();
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
                    parameterList.Add(new ParameterModel
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

                parameterList.Add(new ParameterModel
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

    public static bool IsAbstractModifier(string modifier)
    {
        return CSharpConstants.AbstractIdentifier == modifier;
    }

    private static int CalculateCyclomaticComplexity(ExpressionSyntax conditionExpressionSyntax)
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

    public static IEntityType GetAttributeContainingType(AttributeSyntax syntaxNode, SemanticModel semanticModel)
    {
        var accessorDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<AccessorDeclarationSyntax>();
        if (accessorDeclarationSyntax != null)
        {
            return GetFullName(accessorDeclarationSyntax, semanticModel);
        }

        var propertyDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
        if (propertyDeclarationSyntax != null)
        {
            return GetFullName(propertyDeclarationSyntax, semanticModel);
        }

        var baseMethodDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BaseMethodDeclarationSyntax>();
        if (baseMethodDeclarationSyntax != null)
        {
            return GetFullName(baseMethodDeclarationSyntax, semanticModel);
        }

        var delegateDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<DelegateDeclarationSyntax>();
        if (delegateDeclarationSyntax != null)
        {
            return GetFullName(delegateDeclarationSyntax, semanticModel);
        }

        var parentDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BaseTypeDeclarationSyntax>();
        if (parentDeclarationSyntax != null)
        {
            return GetFullName(parentDeclarationSyntax, semanticModel);
        }

        return CSharpFullNameProvider.CreateEntityTypeModel(syntaxNode.ToString());
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
                    parameterTypes.Add(new ParameterModel
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

                parameterTypes.Add(new ParameterModel
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

        if (attributeListSyntax?.Target == null)
        {
            return "";
        }

        return attributeListSyntax.Target.Identifier.ToString();
    }

    public static AccessedField GetAccessField(ExpressionSyntax identifierNameSyntax, SemanticModel semanticModel)
    {
        var expressionSyntax = identifierNameSyntax;
        if (expressionSyntax == null)
        {
            return null;
        }

        if (identifierNameSyntax is ElementAccessExpressionSyntax elementAccessExpressionSyntax)
        {
            expressionSyntax = elementAccessExpressionSyntax.Expression;
        }

        var symbolInfo = semanticModel.GetSymbolInfo(expressionSyntax);

        if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
        {
            return new AccessedField
            {
                Name = fieldSymbol.Name,
                ContainingTypeName = fieldSymbol.ContainingType.ToString(),
                Kind = GetAccessType(identifierNameSyntax),
            };
        }

        if (symbolInfo.Symbol is IPropertySymbol propertySymbol)
        {
            return new AccessedField
            {
                Name = propertySymbol.Name,
                ContainingTypeName = propertySymbol.ContainingType.ToString(),
                Kind = GetAccessType(identifierNameSyntax),
            };
        }

        if (expressionSyntax is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            if (memberAccessExpressionSyntax.Parent is InvocationExpressionSyntax)
            {
                return null;
            }

            return new AccessedField
            {
                Name = memberAccessExpressionSyntax.Name.ToString(),
                ContainingTypeName = memberAccessExpressionSyntax.Expression.ToString(),
                Kind = GetAccessType(memberAccessExpressionSyntax),
            };
        }

        return null;

        AccessedField.AccessKind GetAccessType(SyntaxNode syntax)
        {
            if (syntax?.Parent is AssignmentExpressionSyntax)
            {
                return AccessedField.AccessKind.Setter;
            }

            return AccessedField.AccessKind.Getter;
        }
    }
}
