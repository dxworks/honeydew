using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction
{
    internal class CSharpFullNameProvider
    {
        private readonly SemanticModel _semanticModel;

        public CSharpFullNameProvider(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public IEntityType GetFullName(SyntaxNode syntaxNode, out bool isNullable)
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
                    var declaredSymbol = _semanticModel.GetDeclaredSymbol(syntaxNode);
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
                    var symbolInfo = _semanticModel.GetSymbolInfo(predefinedTypeSyntax);
                    if (symbolInfo.Symbol != null)
                    {
                        var symbolName = symbolInfo.Symbol.ToString();
                        if (!string.IsNullOrEmpty(symbolName) && symbolName.EndsWith('?'))
                        {
                            isNullable = true;
                        }

                        return CreateEntityTypeModel(symbolName);
                    }

                    var typeInfo = _semanticModel.GetTypeInfo(predefinedTypeSyntax);
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
                    var typeInfo = _semanticModel.GetTypeInfo(baseExpressionSyntax);
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
                    var symbolInfo = _semanticModel.GetSymbolInfo(syntaxNode);
                    if (symbolInfo.Symbol != null)
                    {
                        isNullable = typeSyntax is NullableTypeSyntax;

                        var entityType = GetFullName(symbolInfo.Symbol, false, ref isNullable);

                        return entityType;
                    }

                    switch (typeSyntax)
                    {
                        case RefTypeSyntax refTypeSyntax:
                            return GetFullName(refTypeSyntax.Type, out isNullable);
                        case ArrayTypeSyntax arrayTypeSyntax:
                        {
                            name =
                                $"{GetFullName(arrayTypeSyntax.ElementType, out isNullable).Name}{arrayTypeSyntax.RankSpecifiers.ToString()}";
                        }
                            break;
                        default:
                        {
                            var typeInfo = _semanticModel.GetTypeInfo(typeSyntax);
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
                    var symbolInfo = _semanticModel.GetSymbolInfo(attributeSyntax);
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
                    return GetFullName(variableDeclarationSyntax.Type, out isNullable);
                }

                case TypeConstraintSyntax typeConstraintSyntax:
                {
                    return GetFullName(typeConstraintSyntax.Type, out isNullable);
                }

                case ExpressionSyntax expressionSyntax:
                {
                    var symbolInfo = _semanticModel.GetSymbolInfo(expressionSyntax);
                    if (symbolInfo.Symbol != null)
                    {
                        return GetFullName(symbolInfo.Symbol, false, ref isNullable);
                    }

                    var typeInfo = _semanticModel.GetTypeInfo(expressionSyntax);
                    if (typeInfo.Type != null && typeInfo.Type.ToString() != "?" && typeInfo.Type.ToString() != "?[]")
                    {
                        return GetFullName(typeInfo.Type, false, ref isNullable);
                    }

                    switch (expressionSyntax)
                    {
                        case ObjectCreationExpressionSyntax objectCreationExpressionSyntax:
                            return GetFullName(objectCreationExpressionSyntax.Type, out isNullable);
                        case BaseObjectCreationExpressionSyntax baseObjectCreationExpressionSyntax:
                            return GetFullName(baseObjectCreationExpressionSyntax, out isNullable);
                        case ImplicitArrayCreationExpressionSyntax implicitArrayCreationExpressionSyntax:
                            return GetFullName(implicitArrayCreationExpressionSyntax, out isNullable);
                        case ParenthesizedExpressionSyntax parenthesizedExpressionSyntax:
                            return GetFullName(parenthesizedExpressionSyntax.Expression, out isNullable);
                        case AwaitExpressionSyntax awaitExpressionSyntax:
                        {
                            var entityType = GetFullName(awaitExpressionSyntax.Expression, out isNullable);
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
                        : $"{GetFullName(basePropertyDeclarationSyntax, out isNullable).Name}.{accessorDeclarationSyntax.Keyword.ToString()}";
                }
                    break;

                case ThrowStatementSyntax declarationSyntax:
                {
                    var parentDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<CatchClauseSyntax>();
                    var catchDeclarationSyntax = parentDeclarationSyntax.Declaration;
                    return GetFullName(catchDeclarationSyntax?.Type ?? declarationSyntax.Expression, out isNullable);
                }

                default:
                {
                }
                    break;
            }

            isNullable = !string.IsNullOrEmpty(name) && name.EndsWith('?');

            return CreateEntityTypeModel(name, isExtern);
        }

        private string ReconstructFullName(GenericType genericType)
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

        private IEntityType GetFullName(ISymbol symbolInfo, bool isExternType, ref bool isNullable)
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

        public EntityTypeModel CreateEntityTypeModel(string name, bool isExternType = false)
        {
            try
            {
                return new EntityTypeModel
                {
                    Name = name,
                    FullType = GetFullType(name),
                    IsExtern = isExternType
                };
            }
            catch (Exception)
            {
                return new EntityTypeModel
                {
                    Name = name,
                    FullType = new GenericType
                    {
                        Name = name
                    },
                    IsExtern = isExternType
                };
            }
        }

        private GenericType GetFullType(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new GenericType
                {
                    Name = name
                };
            }

            var isNullable = false;

            if (name.EndsWith('?'))
            {
                isNullable = true;
                name = name[..^1];
            }

            ReadOnlySpan<char> span = name;

            var fullType = GetFullType(span);
            fullType.IsNullable = isNullable;

            return fullType;
        }

        private GenericType GetFullType(ReadOnlySpan<char> name)
        {
            if (!name.Contains('<'))
            {
                var trimmedName = name.ToString().Trim();
                var isNullable = false;
                if (trimmedName.EndsWith('?'))
                {
                    isNullable = true;
                    trimmedName = trimmedName[..^1];
                }

                return new GenericType
                {
                    Name = trimmedName,
                    IsNullable = isNullable
                };
            }

            var genericType = new GenericType
            {
                IsNullable = name[^1] == '?'
            };

            var genericStart = name.IndexOf('<');
            var genericEnd = name.LastIndexOf('>');

            genericType.Name = name[..genericStart].ToString().Trim();


            ReadOnlySpan<char> span = name;

            var commaIndices = new List<int>
            {
                genericStart
            };

            var angleBracketCount = 0;

            for (var i = genericStart + 1; i < genericEnd; i++)
            {
                switch (span[i])
                {
                    case '<':
                        angleBracketCount++;
                        break;
                    case '>':
                        angleBracketCount--;
                        break;
                    case ',':
                    {
                        if (angleBracketCount == 0)
                        {
                            commaIndices.Add(i);
                        }

                        break;
                    }
                }
            }

            commaIndices.Add(genericEnd);

            for (var i = 0; i < commaIndices.Count - 1; i++)
            {
                var part = span.Slice(commaIndices[i] + 1, commaIndices[i + 1] - commaIndices[i] - 1);
                genericType.ContainedTypes.Add(GetFullType(part));
            }

            return genericType;
        }

        private IEntityType GetFullName(BaseObjectCreationExpressionSyntax declarationSyntax, out bool isNullable)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(declarationSyntax);

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
                return GetFullName(variableDeclarationSyntax, out isNullable);
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
                return GetFullName(objectCreationExpressionSyntax.Type, out isNullable);
            }

            name = declarationSyntax.ToString();
            isNullable = name.EndsWith('?');

            return CreateEntityTypeModel(name);
        }

        private IEntityType GetFullName(ImplicitArrayCreationExpressionSyntax declarationSyntax, out bool isNullable)
        {
            var basePropertyDeclarationSyntax =
                declarationSyntax.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
            if (basePropertyDeclarationSyntax != null)
            {
                return GetFullName(basePropertyDeclarationSyntax.Type, out isNullable);
            }

            var baseFieldDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<BaseFieldDeclarationSyntax>();
            if (baseFieldDeclarationSyntax != null)
            {
                return GetFullName(baseFieldDeclarationSyntax, out isNullable);
            }

            var variableDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<VariableDeclarationSyntax>();
            if (variableDeclarationSyntax != null)
            {
                return GetFullName(variableDeclarationSyntax, out isNullable);
            }

            // try to infer type from elements
            var elementTypesSet = new HashSet<string>();

            foreach (var expression in declarationSyntax.Initializer.Expressions)
            {
                var fullName = GetExpressionType(expression, out isNullable).Name;
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

        private IEntityType GetExpressionType(ExpressionSyntax expression, out bool isNullable)
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
                    return GetFullName(objectCreationExpressionSyntax, out isNullable);
                }
            }

            return GetFullName(expression, out isNullable);
        }
    }
}
