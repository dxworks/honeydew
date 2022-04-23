using System.Text;
using Honeydew.Extractors.CSharp.Utils;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Utils;

internal static class CSharpFullNameProvider
{
    public static IEntityType GetFullName(SyntaxNode syntaxNode, SemanticModel semanticModel, out bool isNullable)
    {
        var name = syntaxNode.ToString();
        var isExtern = false;
        isNullable = false;

        switch (syntaxNode)
        {
            case BaseTypeDeclarationSyntax:
            case DelegateDeclarationSyntax:
            case BaseMethodDeclarationSyntax:
            case BasePropertyDeclarationSyntax:
            {
                var declaredSymbol = semanticModel.GetDeclaredSymbol(syntaxNode);
                if (declaredSymbol != null)
                {
                    name = declaredSymbol.ToDisplayString();
                }

                var typeParameterListSyntax =
                    syntaxNode.ChildNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
                if (typeParameterListSyntax != null)
                {
                    var indexOf = name.IndexOf('<');
                    name = $"{name[..indexOf]}{typeParameterListSyntax.ToString()}";
                }
            }
                break;

            case PredefinedTypeSyntax predefinedTypeSyntax:
            {
                var symbolInfo = semanticModel.GetSymbolInfo(predefinedTypeSyntax);
                if (symbolInfo.Symbol != null)
                {
                    var symbolName = symbolInfo.Symbol.ToString();
                    if (!string.IsNullOrEmpty(symbolName) && symbolName.EndsWith('?'))
                    {
                        isNullable = true;
                    }

                    return CreateEntityTypeModel(symbolName);
                }

                var typeInfo = semanticModel.GetTypeInfo(predefinedTypeSyntax);
                if (typeInfo.Type != null)
                {
                    var typeName = typeInfo.Type.ToString();
                    if (!string.IsNullOrEmpty(typeName) && typeName.EndsWith('?'))
                    {
                        isNullable = true;
                    }

                    return CreateEntityTypeModel(typeName);
                }

                name = "";
                isExtern = true;
            }
                break;

            case BaseExpressionSyntax baseExpressionSyntax:
            {
                var typeInfo = semanticModel.GetTypeInfo(baseExpressionSyntax);
                if (typeInfo.Type != null)
                {
                    var typeName = typeInfo.Type.ToDisplayString();
                    if (!string.IsNullOrEmpty(typeName) && typeName.EndsWith('?'))
                    {
                        isNullable = true;
                    }

                    return CreateEntityTypeModel(typeName);
                }
            }
                break;

            case TypeSyntax typeSyntax:
            {
                var symbolInfo = semanticModel.GetSymbolInfo(syntaxNode);
                if (symbolInfo.Symbol != null)
                {
                    isNullable = typeSyntax is NullableTypeSyntax;

                    var entityType = GetFullName(symbolInfo.Symbol, false, ref isNullable);

                    return entityType;
                }

                switch (typeSyntax)
                {
                    case RefTypeSyntax refTypeSyntax:
                        return GetFullName(refTypeSyntax.Type, semanticModel, out isNullable);
                    case ArrayTypeSyntax arrayTypeSyntax:
                    {
                        name =
                            $"{GetFullName(arrayTypeSyntax.ElementType, semanticModel, out isNullable).Name}{arrayTypeSyntax.RankSpecifiers.ToString()}";
                    }
                        break;
                    default:
                    {
                        var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
                        if (typeInfo.Type != null && typeInfo.Type.ToString() != "?" &&
                            typeInfo.Type.ToString() != "?[]")
                        {
                            name = typeInfo.Type.ToString();
                        }
                        else
                        {
                            name = typeSyntax.ToString();
                            isExtern = true;
                        }
                    }
                        break;
                }
            }
                break;

            case AttributeSyntax attributeSyntax:
            {
                var symbolInfo = semanticModel.GetSymbolInfo(attributeSyntax);
                if (symbolInfo.Symbol != null)
                {
                    name = symbolInfo.Symbol.ContainingSymbol.ToDisplayString();
                }
                else
                {
                    name = attributeSyntax.Name.ToString();
                    isExtern = true;
                }
            }
                break;

            case VariableDeclarationSyntax variableDeclarationSyntax:
            {
                return GetFullName(variableDeclarationSyntax.Type, semanticModel, out isNullable);
            }

            case TypeConstraintSyntax typeConstraintSyntax:
            {
                return GetFullName(typeConstraintSyntax.Type, semanticModel, out isNullable);
            }

            case ExpressionSyntax expressionSyntax:
            {
                var symbolInfo = semanticModel.GetSymbolInfo(expressionSyntax);
                if (symbolInfo.Symbol != null)
                {
                    return GetFullName(symbolInfo.Symbol, false, ref isNullable);
                }

                var typeInfo = semanticModel.GetTypeInfo(expressionSyntax);
                if (typeInfo.Type != null && typeInfo.Type.ToString() != "?" && typeInfo.Type.ToString() != "?[]")
                {
                    return GetFullName(typeInfo.Type, false, ref isNullable);
                }

                switch (expressionSyntax)
                {
                    case ObjectCreationExpressionSyntax objectCreationExpressionSyntax:
                        return GetFullName(objectCreationExpressionSyntax.Type, semanticModel, out isNullable);
                    case BaseObjectCreationExpressionSyntax baseObjectCreationExpressionSyntax:
                        return GetFullName(baseObjectCreationExpressionSyntax, semanticModel, out isNullable);
                    case ImplicitArrayCreationExpressionSyntax implicitArrayCreationExpressionSyntax:
                        return GetFullName(implicitArrayCreationExpressionSyntax, semanticModel, out isNullable);
                    case ParenthesizedExpressionSyntax parenthesizedExpressionSyntax:
                        return GetFullName(parenthesizedExpressionSyntax.Expression, semanticModel, out isNullable);
                    case AwaitExpressionSyntax awaitExpressionSyntax:
                    {
                        var entityType = GetFullName(awaitExpressionSyntax.Expression, semanticModel, out isNullable);
                        if (entityType.FullType.ContainedTypes.Count > 0)
                        {
                            var fullName = ReconstructFullName(entityType.FullType.ContainedTypes[0]);
                            return CreateEntityTypeModel(fullName);
                        }

                        return entityType;
                    }
                    case TypeOfExpressionSyntax:
                    {
                        name = "System.Type";
                    }
                        break;
                    default:
                    {
                        name = "";
                        isExtern = true;
                    }
                        break;
                }
            }
                break;

            case AccessorDeclarationSyntax accessorDeclarationSyntax:
            {
                var basePropertyDeclarationSyntax = accessorDeclarationSyntax
                    .GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
                name = basePropertyDeclarationSyntax == null
                    ? accessorDeclarationSyntax.Keyword.ToString()
                    : $"{GetFullName(basePropertyDeclarationSyntax, semanticModel, out isNullable).Name}.{accessorDeclarationSyntax.Keyword.ToString()}";
            }
                break;

            case ThrowStatementSyntax declarationSyntax:
            {
                var parentDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<CatchClauseSyntax>();
                var catchDeclarationSyntax = parentDeclarationSyntax.Declaration;
                return GetFullName(catchDeclarationSyntax?.Type ?? declarationSyntax.Expression, semanticModel,
                    out isNullable);
            }

            default:
            {
            }
                break;
        }

        isNullable = !string.IsNullOrEmpty(name) && name.EndsWith('?');

        return CreateEntityTypeModel(name, isExtern);
    }

    public static IEntityType CreateEntityTypeModel(string name, bool isExternType = false)
    {
        return FullTypeNameBuilder.CreateEntityTypeModel(name, isExternType);
    }

    private static string ReconstructFullName(GenericType genericType)
    {
        if (genericType == null)
        {
            return "";
        }

        var stringBuilder = new StringBuilder();
        var name = genericType.Name;

        stringBuilder.Append(name);

        if (genericType.ContainedTypes.Count <= 0)
        {
            return stringBuilder.ToString();
        }

        stringBuilder.Append('<');
        for (var i = 0; i < genericType.ContainedTypes.Count; i++)
        {
            var containedType = genericType.ContainedTypes[i];
            stringBuilder.Append(ReconstructFullName(containedType));
            if (i != genericType.ContainedTypes.Count - 1)
            {
                stringBuilder.Append(", ");
            }
        }

        stringBuilder.Append('>');

        return stringBuilder.ToString();
    }

    private static IEntityType GetFullName(ISymbol symbolInfo, bool isExternType, ref bool isNullable)
    {
        if (symbolInfo == null)
        {
            return new EntityTypeModel
            {
                Name = ""
            };
        }

        var name = symbolInfo.ToDisplayString();

        switch (symbolInfo)
        {
            case IPropertySymbol propertySymbol:
            {
                name = propertySymbol.Type.ToDisplayString();
            }
                break;
            case ILocalSymbol localSymbol:
            {
                name = localSymbol.Type.ToDisplayString();
            }
                break;
            case IFieldSymbol fieldSymbol:
            {
                name = fieldSymbol.Type.ToDisplayString();
            }
                break;
            case IMethodSymbol methodSymbol:
            {
                if (methodSymbol.MethodKind == MethodKind.Constructor && methodSymbol.ReceiverType != null)
                {
                    name = methodSymbol.ReceiverType.ToDisplayString();
                }
                else
                {
                    name = methodSymbol.ReturnType.ToDisplayString();
                }
            }
                break;

            case IParameterSymbol parameterSymbol:
            {
                name = parameterSymbol.Type.ToDisplayString();
            }
                break;
        }

        if (name.EndsWith('?'))
        {
            isNullable = true;
        }
        else
        {
            if (isNullable)
            {
                name += '?';
            }
        }

        return CreateEntityTypeModel(name, isExternType);
    }

    private static IEntityType GetFullName(BaseObjectCreationExpressionSyntax declarationSyntax,
        SemanticModel semanticModel, out bool isNullable)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(declarationSyntax);

        string name;
        if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
        {
            name = methodSymbol.ContainingType.ToDisplayString();
            isNullable = name.EndsWith('?');

            if (isNullable)
            {
                name = name[..^1];
            }

            return CreateEntityTypeModel(name);
        }

        var variableDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<VariableDeclarationSyntax>();
        if (variableDeclarationSyntax != null)
        {
            return GetFullName(variableDeclarationSyntax, semanticModel, out isNullable);
        }

        var propertyDeclarationSyntax =
            declarationSyntax.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
        if (propertyDeclarationSyntax != null)
        {
            name = propertyDeclarationSyntax.Type.ToString();
            isNullable = name.EndsWith('?');

            return CreateEntityTypeModel(name);
        }

        if (declarationSyntax is ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
        {
            return GetFullName(objectCreationExpressionSyntax.Type, semanticModel, out isNullable);
        }

        name = declarationSyntax.ToString();
        isNullable = name.EndsWith('?');

        return CreateEntityTypeModel(name);
    }

    private static IEntityType GetFullName(ImplicitArrayCreationExpressionSyntax declarationSyntax,
        SemanticModel semanticModel, out bool isNullable)
    {
        var basePropertyDeclarationSyntax =
            declarationSyntax.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
        if (basePropertyDeclarationSyntax != null)
        {
            return GetFullName(basePropertyDeclarationSyntax.Type, semanticModel, out isNullable);
        }

        var baseFieldDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<BaseFieldDeclarationSyntax>();
        if (baseFieldDeclarationSyntax != null)
        {
            return GetFullName(baseFieldDeclarationSyntax, semanticModel, out isNullable);
        }

        var variableDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<VariableDeclarationSyntax>();
        if (variableDeclarationSyntax != null)
        {
            return GetFullName(variableDeclarationSyntax, semanticModel, out isNullable);
        }

        // try to infer type from elements
        var elementTypesSet = new HashSet<string>();

        foreach (var expression in declarationSyntax.Initializer.Expressions)
        {
            var fullName = GetExpressionType(expression, semanticModel, out isNullable).Name;
            elementTypesSet.Add(fullName);
        }

        isNullable = false;

        switch (elementTypesSet.Count)
        {
            case 0:
            {
                return new EntityTypeModel
                {
                    Name = declarationSyntax.ToString()
                };
            }
            case 1:
            {
                return new EntityTypeModel
                {
                    Name = $"{elementTypesSet.First()}[]"
                };
            }
            case 2:
            {
                if (elementTypesSet.Contains("System.Int32") && elementTypesSet.Contains("System.Single"))
                {
                    return new EntityTypeModel
                    {
                        Name = "System.Single[]"
                    };
                }

                if (elementTypesSet.Contains("System.Int32") && elementTypesSet.Contains("System.Double"))
                {
                    return new EntityTypeModel
                    {
                        Name = "System.Double[]"
                    };
                }

                if (elementTypesSet.Contains("System.Single") && elementTypesSet.Contains("System.Double"))
                {
                    return new EntityTypeModel
                    {
                        Name = "System.Double[]"
                    };
                }

                return new EntityTypeModel
                {
                    Name = "System.Object[]"
                };
            }
            case 3:
            {
                if (elementTypesSet.Contains("System.Int32") && elementTypesSet.Contains("System.Single") &&
                    elementTypesSet.Contains("System.Double"))
                {
                    return new EntityTypeModel
                    {
                        Name = "System.Double[]"
                    };
                }

                return new EntityTypeModel
                {
                    Name = "System.Object[]"
                };
            }
            default:
                return new EntityTypeModel
                {
                    Name = "System.Object[]"
                };
        }
    }

    private static IEntityType GetExpressionType(ExpressionSyntax expression, SemanticModel semanticModel,
        out bool isNullable)
    {
        switch (expression)
        {
            case LiteralExpressionSyntax literalExpressionSyntax:
            {
                if (literalExpressionSyntax.Token.Value != null)
                {
                    var name = literalExpressionSyntax.Token.Value.GetType().FullName;
                    isNullable = !string.IsNullOrEmpty(name) && name.EndsWith('?');

                    return CreateEntityTypeModel(name);
                }
            }
                break;

            case ObjectCreationExpressionSyntax objectCreationExpressionSyntax:
            {
                return GetFullName(objectCreationExpressionSyntax, semanticModel, out isNullable);
            }
        }

        return GetFullName(expression, semanticModel, out isNullable);
    }
}
