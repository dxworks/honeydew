﻿using System.Text;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Utils;

internal static class VisualBasicFullNameProvider
{
    public static IEntityType GetFullName(SyntaxNode syntaxNode, SemanticModel semanticModel, out bool isNullable)
    {
        var name = syntaxNode.ToString();
        var isExtern = false;
        isNullable = false;

        switch (syntaxNode)
        {
            case TypeStatementSyntax:
            case EnumStatementSyntax:
            case DelegateStatementSyntax:
                // case BaseMethodDeclarationSyntax:
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
                    name = $"{name[..indexOf]}{typeParameterListSyntax}";
                }
            }
                break;

            case TypeBlockSyntax typeBlockSyntax:
            {
                return GetFullName(typeBlockSyntax.BlockStatement, semanticModel, out isNullable);
            }

            case PropertyStatementSyntax propertyStatementSyntax:
            {
                switch (propertyStatementSyntax.AsClause)
                {
                    case AsNewClauseSyntax asNewClauseSyntax:
                        return GetFullName(asNewClauseSyntax.NewExpression, semanticModel, out isNullable);
                    case SimpleAsClauseSyntax simpleAsClauseSyntax:
                        if (simpleAsClauseSyntax.Type is not null)
                        {
                            return CreateEntityTypeModel(simpleAsClauseSyntax.Type.ToString(), isExtern);
                        }

                        break;
                }
            }
                break;
            //
            // case BaseExpressionSyntax baseExpressionSyntax:
            // {
            //     var typeInfo = semanticModel.GetTypeInfo(baseExpressionSyntax);
            //     if (typeInfo.Type != null)
            //     {
            //         var typeName = typeInfo.Type.ToDisplayString();
            //         if (!string.IsNullOrEmpty(typeName) && typeName.EndsWith('?'))
            //         {
            //             isNullable = true;
            //         }
            //
            //         return CreateEntityTypeModel(typeName);
            //     }
            // }
            // break;

            // case TypeSyntax typeSyntax:
            // {
            //     var symbolInfo = semanticModel.GetSymbolInfo(syntaxNode);
            //     if (symbolInfo.Symbol != null)
            //     {
            //         isNullable = typeSyntax is NullableTypeSyntax;
            //
            //         var entityType = GetFullName(symbolInfo.Symbol, false, ref isNullable);
            //
            //         return entityType;
            //     }
            //
            //     switch (typeSyntax)
            //     {
            //         case RefTypeSyntax refTypeSyntax:
            //             return GetFullName(refTypeSyntax.Type, semanticModel, out isNullable);
            //         case ArrayTypeSyntax arrayTypeSyntax:
            //         {
            //             name =
            //                 $"{GetFullName(arrayTypeSyntax.ElementType, semanticModel, out isNullable).Name}{arrayTypeSyntax.RankSpecifiers.ToString()}";
            //         }
            //             break;
            //         default:
            //         {
            //             var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
            //             if (typeInfo.Type != null && typeInfo.Type.ToString() != "?" &&
            //                 typeInfo.Type.ToString() != "?[]")
            //             {
            //                 name = typeInfo.Type.ToString();
            //             }
            //             else
            //             {
            //                 name = typeSyntax.ToString();
            //                 isExtern = true;
            //             }
            //         }
            //             break;
            //     }
            // }
            //     break;

            // case AttributeSyntax attributeSyntax:
            // {
            //     var symbolInfo = semanticModel.GetSymbolInfo(attributeSyntax);
            //     if (symbolInfo.Symbol != null)
            //     {
            //         name = symbolInfo.Symbol.ContainingSymbol.ToDisplayString();
            //     }
            //     else
            //     {
            //         name = attributeSyntax.Name.ToString();
            //         isExtern = true;
            //     }
            // }
            //     break;
            //
            case VariableDeclaratorSyntax variableDeclarationSyntax:
            {
                switch (variableDeclarationSyntax.AsClause)
                {
                    case AsNewClauseSyntax asNewClauseSyntax:
                        return GetFullName(asNewClauseSyntax.NewExpression, semanticModel, out isNullable);
                    case SimpleAsClauseSyntax simpleAsClauseSyntax:
                        return GetFullName(simpleAsClauseSyntax.Type, semanticModel, out isNullable);
                }

                // return GetFullName(variableDeclarationSyntax.Type, semanticModel, out isNullable);
            }
                break;
            //
            // case TypeConstraintSyntax typeConstraintSyntax:
            // {
            //     return GetFullName(typeConstraintSyntax.Type, semanticModel, out isNullable);
            // }
            //
            case ExpressionSyntax expressionSyntax:
            {
                var symbolInfo = semanticModel.GetSymbolInfo(expressionSyntax);
                if (symbolInfo.Symbol != null)
                {
                    return GetFullName(symbolInfo.Symbol, false, ref isNullable);
                }

                switch (expressionSyntax)
                {
                    case ObjectCreationExpressionSyntax objectCreationExpressionSyntax:
                        return CreateEntityTypeModel(objectCreationExpressionSyntax.Type.ToString(), isExtern);
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
                    case IdentifierNameSyntax identifierNameSyntax:
                    {
                        name = identifierNameSyntax.Identifier.ToString();
                        isExtern = true;
                    }
                        break;
                    case GenericNameSyntax genericNameSyntax:
                    {
                        return GetGenericFullName(genericNameSyntax, semanticModel, out isNullable);
                    }
                    default:
                    {
                        name = "";
                        isExtern = true;
                    }
                        break;
                }
            }
                break;

            // case AccessorDeclarationSyntax accessorDeclarationSyntax:
            // {
            //     var basePropertyDeclarationSyntax = accessorDeclarationSyntax
            //         .GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
            //     name = basePropertyDeclarationSyntax == null
            //         ? accessorDeclarationSyntax.Keyword.ToString()
            //         : $"{GetFullName(basePropertyDeclarationSyntax, semanticModel, out isNullable).Name}.{accessorDeclarationSyntax.Keyword.ToString()}";
            // }
            //     break;

            default:
            {
            }
                break;
        }

        isNullable = !string.IsNullOrEmpty(name) && name.EndsWith('?');

        return CreateEntityTypeModel(name, isExtern);
    }

    private static IEntityType GetGenericFullName(GenericNameSyntax genericNameSyntax, SemanticModel semanticModel,
        out bool isNullable)
    {
        var name = genericNameSyntax.ToString();
        isNullable = name.EndsWith('?');
        if (isNullable)
        {
            name = name.TrimEnd('?');
        }

        return new VisualBasicEntityTypeModel
        {
            Name = name,
            FullType = new GenericType
            {
                Name = name,
                ContainedTypes = genericNameSyntax.TypeArgumentList.Arguments
                    .Select(arg =>
                    {
                        switch (arg)
                        {
                            case GenericNameSyntax genericNameSyntax1:
                                return GetGenericFullName(genericNameSyntax1, semanticModel, out _).FullType;
                            default:
                                var argName = arg.ToString();
                                var nullable = argName.EndsWith('?');
                                if (nullable)
                                {
                                    argName = argName.TrimEnd('?');
                                }

                                return new GenericType
                                {
                                    Name = argName,
                                    IsNullable = nullable
                                };
                        }
                    })
                    .ToList(),
                IsNullable = isNullable
            }
        };
    }

    public static IEntityType CreateEntityTypeModel(string? name, bool isExternType = false)
    {
        return VisualBasicFullTypeNameBuilder.CreateEntityTypeModel(name, isExternType);
    }

    private static string ReconstructFullName(GenericType? genericType)
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

    private static IEntityType GetFullName(ISymbol? symbolInfo, bool isExternType, ref bool isNullable)
    {
        if (symbolInfo == null)
        {
            return new VisualBasicEntityTypeModel
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

    // private static IEntityType GetFullName(ObjectCreationExpressionSyntax declarationSyntax,
    //     SemanticModel semanticModel, out bool isNullable)
    // {
    //     var symbolInfo = semanticModel.GetSymbolInfo(declarationSyntax);
    //
    //     string name;
    //     if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
    //     {
    //         name = methodSymbol.ContainingType.ToDisplayString();
    //         isNullable = name.EndsWith('?');
    //
    //         if (isNullable)
    //         {
    //             name = name[..^1];
    //         }
    //
    //         return CreateEntityTypeModel(name);
    //     }
    //
    //     var variableDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<VariableDeclarationSyntax>();
    //     if (variableDeclarationSyntax != null)
    //     {
    //         return GetFullName(variableDeclarationSyntax, semanticModel, out isNullable);
    //     }
    //
    //     var propertyDeclarationSyntax =
    //         declarationSyntax.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
    //     if (propertyDeclarationSyntax != null)
    //     {
    //         name = propertyDeclarationSyntax.Type.ToString();
    //         isNullable = name.EndsWith('?');
    //
    //         return CreateEntityTypeModel(name);
    //     }
    //
    //     if (declarationSyntax is ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
    //     {
    //         return GetFullName(objectCreationExpressionSyntax.Type, semanticModel, out isNullable);
    //     }
    //
    //     name = declarationSyntax.ToString();
    //     isNullable = name.EndsWith('?');
    //
    //     return CreateEntityTypeModel(name);
    // }

    // private static IEntityType GetFullName(ImplicitArrayCreationExpressionSyntax declarationSyntax,
    //     SemanticModel semanticModel, out bool isNullable)
    // {
    //     var basePropertyDeclarationSyntax =
    //         declarationSyntax.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
    //     if (basePropertyDeclarationSyntax != null)
    //     {
    //         return GetFullName(basePropertyDeclarationSyntax.Type, semanticModel, out isNullable);
    //     }
    //
    //     var baseFieldDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<BaseFieldDeclarationSyntax>();
    //     if (baseFieldDeclarationSyntax != null)
    //     {
    //         return GetFullName(baseFieldDeclarationSyntax, semanticModel, out isNullable);
    //     }
    //
    //     var variableDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<VariableDeclarationSyntax>();
    //     if (variableDeclarationSyntax != null)
    //     {
    //         return GetFullName(variableDeclarationSyntax, semanticModel, out isNullable);
    //     }
    //
    //     // try to infer type from elements
    //     var elementTypesSet = new HashSet<string>();
    //
    //     foreach (var expression in declarationSyntax.Initializer.Expressions)
    //     {
    //         var fullName = GetExpressionType(expression, semanticModel, out isNullable).Name;
    //         elementTypesSet.Add(fullName);
    //     }
    //
    //     isNullable = false;
    //
    //     switch (elementTypesSet.Count)
    //     {
    //         case 0:
    //         {
    //             return new VisualBasicEntityTypeModel
    //             {
    //                 Name = declarationSyntax.ToString()
    //             };
    //         }
    //         case 1:
    //         {
    //             return new VisualBasicEntityTypeModel
    //             {
    //                 Name = $"{elementTypesSet.First()}[]"
    //             };
    //         }
    //         case 2:
    //         {
    //             if (elementTypesSet.Contains("System.Int32") && elementTypesSet.Contains("System.Single"))
    //             {
    //                 return new VisualBasicEntityTypeModel
    //                 {
    //                     Name = "System.Single[]"
    //                 };
    //             }
    //
    //             if (elementTypesSet.Contains("System.Int32") && elementTypesSet.Contains("System.Double"))
    //             {
    //                 return new VisualBasicEntityTypeModel
    //                 {
    //                     Name = "System.Double[]"
    //                 };
    //             }
    //
    //             if (elementTypesSet.Contains("System.Single") && elementTypesSet.Contains("System.Double"))
    //             {
    //                 return new VisualBasicEntityTypeModel
    //                 {
    //                     Name = "System.Double[]"
    //                 };
    //             }
    //
    //             return new VisualBasicEntityTypeModel
    //             {
    //                 Name = "System.Object[]"
    //             };
    //         }
    //         case 3:
    //         {
    //             if (elementTypesSet.Contains("System.Int32") && elementTypesSet.Contains("System.Single") &&
    //                 elementTypesSet.Contains("System.Double"))
    //             {
    //                 return new VisualBasicEntityTypeModel
    //                 {
    //                     Name = "System.Double[]"
    //                 };
    //             }
    //
    //             return new VisualBasicEntityTypeModel
    //             {
    //                 Name = "System.Object[]"
    //             };
    //         }
    //         default:
    //             return new VisualBasicEntityTypeModel
    //             {
    //                 Name = "System.Object[]"
    //             };
    //     }
    // }

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
