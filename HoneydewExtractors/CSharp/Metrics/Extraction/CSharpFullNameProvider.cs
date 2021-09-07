using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEntityType GetFullName(SyntaxNode syntaxNode)
        {
            var name = syntaxNode.ToString();

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

                case TypeSyntax typeSyntax:
                {
                    var symbolInfo = _semanticModel.GetSymbolInfo(syntaxNode);
                    if (symbolInfo.Symbol != null)
                    {
                        return GetFullName(symbolInfo.Symbol);
                    }

                    switch (typeSyntax)
                    {
                        case RefTypeSyntax refTypeSyntax:
                            return GetFullName(refTypeSyntax.Type);
                        case ArrayTypeSyntax arrayTypeSyntax:
                        {
                            name =
                                $"{GetFullName(arrayTypeSyntax.ElementType).Name}{arrayTypeSyntax.RankSpecifiers.ToString()}";
                        }
                            break;
                        default:
                        {
                            name = typeSyntax.ToString();
                        }
                            break;
                    }
                }
                    break;

                case AttributeSyntax attributeSyntax:
                {
                    var symbolInfo = _semanticModel.GetSymbolInfo(attributeSyntax);
                    name = symbolInfo.Symbol != null
                        ? symbolInfo.Symbol.ContainingType.ToString()
                        : attributeSyntax.Name.ToString();
                }
                    break;

                case VariableDeclarationSyntax variableDeclarationSyntax:
                {
                    return GetFullName(variableDeclarationSyntax.Type);
                }

                case TypeConstraintSyntax typeConstraintSyntax:
                {
                    return GetFullName(typeConstraintSyntax.Type);
                }

                case ExpressionSyntax expressionSyntax:
                {
                    var symbolInfo = _semanticModel.GetSymbolInfo(expressionSyntax);
                    if (symbolInfo.Symbol != null)
                    {
                        return GetFullName(symbolInfo.Symbol);
                    }

                    switch (expressionSyntax)
                    {
                        case ObjectCreationExpressionSyntax objectCreationExpressionSyntax:
                            return GetFullName(objectCreationExpressionSyntax.Type);
                        case BaseObjectCreationExpressionSyntax baseObjectCreationExpressionSyntax:
                            return GetFullName(baseObjectCreationExpressionSyntax);
                        case ImplicitArrayCreationExpressionSyntax implicitArrayCreationExpressionSyntax:
                            return GetFullName(implicitArrayCreationExpressionSyntax);
                        // local variable type
                        // case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                        //     return GetFullName(memberAccessExpressionSyntax.Name);
                    }
                }
                    break;

                case AccessorDeclarationSyntax accessorDeclarationSyntax:
                {
                    var basePropertyDeclarationSyntax = accessorDeclarationSyntax
                        .GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
                    name = basePropertyDeclarationSyntax == null
                        ? accessorDeclarationSyntax.Keyword.ToString()
                        : $"{GetFullName(basePropertyDeclarationSyntax).Name}.{accessorDeclarationSyntax.Keyword.ToString()}";
                }
                    break;

                case ThrowStatementSyntax declarationSyntax:
                {
                    var parentDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<CatchClauseSyntax>();
                    var catchDeclarationSyntax = parentDeclarationSyntax.Declaration;
                    return GetFullName(catchDeclarationSyntax?.Type ?? declarationSyntax.Expression);
                }
            }

            return CreateEntityTypeModel(name);
        }

        private IEntityType GetFullName(ISymbol symbolInfo)
        {
            if (symbolInfo == null)
            {
                return new EntityTypeModel
                {
                    Name = ""
                };
            }

            var name = symbolInfo.ToString();

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

            return CreateEntityTypeModel(name);
        }

        public EntityTypeModel CreateEntityTypeModel(string name)
        {
            try
            {
                return new EntityTypeModel
                {
                    Name = name,
                    FullType = GetContainedTypes(name)
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
                    }
                };
            }
        }

        private GenericType GetContainedTypes(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new GenericType
                {
                    Name = name
                };
            }

            ReadOnlySpan<char> span = name;
            return GetContainedTypes(span);
        }

        private GenericType GetContainedTypes(ReadOnlySpan<char> name)
        {
            if (!name.Contains('<'))
            {
                return new GenericType
                {
                    Name = name.ToString().Trim()
                };
            }

            var genericType = new GenericType();

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
                genericType.ContainedTypes.Add(GetContainedTypes(part));
            }

            return genericType;
        }

        private IEntityType GetFullName(BaseObjectCreationExpressionSyntax declarationSyntax)
        {
            var symbolInfo = ModelExtensions.GetSymbolInfo(_semanticModel, declarationSyntax);

            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                return new EntityTypeModel
                {
                    Name = methodSymbol.ContainingType.ToDisplayString()
                };
            }

            var variableDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<VariableDeclarationSyntax>();
            if (variableDeclarationSyntax != null)
            {
                return GetFullName(variableDeclarationSyntax);
            }

            var propertyDeclarationSyntax =
                declarationSyntax.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
            if (propertyDeclarationSyntax != null)
            {
                return new EntityTypeModel
                {
                    Name = propertyDeclarationSyntax.Type.ToString()
                };
            }

            if (declarationSyntax is ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
            {
                return GetFullName(objectCreationExpressionSyntax.Type);
            }

            return new EntityTypeModel
            {
                Name = declarationSyntax.ToString()
            };
        }


        private IEntityType GetFullName(ImplicitArrayCreationExpressionSyntax declarationSyntax)
        {
            var basePropertyDeclarationSyntax =
                declarationSyntax.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
            if (basePropertyDeclarationSyntax != null)
            {
                return GetFullName(basePropertyDeclarationSyntax.Type);
            }

            var baseFieldDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<BaseFieldDeclarationSyntax>();
            if (baseFieldDeclarationSyntax != null)
            {
                return GetFullName(baseFieldDeclarationSyntax);
            }

            var variableDeclarationSyntax = declarationSyntax.GetParentDeclarationSyntax<VariableDeclarationSyntax>();
            if (variableDeclarationSyntax != null)
            {
                return GetFullName(variableDeclarationSyntax);
            }

            // try to infer type from elements
            var elementTypesSet = new HashSet<string>();

            foreach (var expression in declarationSyntax.Initializer.Expressions)
            {
                var fullName = GetExpressionType(expression).Name;
                elementTypesSet.Add(fullName);
            }

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

        private IEntityType GetExpressionType(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literalExpressionSyntax:
                {
                    if (literalExpressionSyntax.Token.Value != null)
                    {
                        return new EntityTypeModel
                        {
                            Name = literalExpressionSyntax.Token.Value.GetType().FullName
                        };
                    }
                }
                    break;

                case ObjectCreationExpressionSyntax objectCreationExpressionSyntax:
                {
                    return GetFullName(objectCreationExpressionSyntax);
                }
            }

            return GetFullName(expression);
        }
    }
}
