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
            var entityTypeModel = new EntityTypeModel
            {
                Name = syntaxNode.ToString()
            };

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
                        entityTypeModel.Name = declaredSymbol.ToDisplayString();
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
                            entityTypeModel.Name =
                                $"{GetFullName(arrayTypeSyntax.ElementType).Name}{arrayTypeSyntax.RankSpecifiers.ToString()}";
                        }
                            break;
                        default:
                        {
                            entityTypeModel.Name = typeSyntax.ToString();
                        }
                            break;
                    }
                }
                    break;

                case AttributeSyntax attributeSyntax:
                {
                    var symbolInfo = _semanticModel.GetSymbolInfo(attributeSyntax);
                    entityTypeModel.Name = symbolInfo.Symbol != null
                        ? symbolInfo.Symbol.ContainingType.ToString()
                        : attributeSyntax.Name.ToString();
                }
                    break;

                case VariableDeclarationSyntax variableDeclarationSyntax:
                {
                    return GetFullName(variableDeclarationSyntax.Type);
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
                    }
                }
                    break;

                case AccessorDeclarationSyntax accessorDeclarationSyntax:
                {
                    var basePropertyDeclarationSyntax = accessorDeclarationSyntax
                        .GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
                    entityTypeModel.Name = basePropertyDeclarationSyntax == null
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

            return entityTypeModel;
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

            switch (symbolInfo)
            {
                case IPropertySymbol propertySymbol:
                {
                    return new EntityTypeModel
                    {
                        Name = propertySymbol.Type.ToDisplayString()
                    };
                }
                case ILocalSymbol localSymbol:
                {
                    return new EntityTypeModel
                    {
                        Name = localSymbol.Type.ToDisplayString()
                    };
                }
                case IFieldSymbol fieldSymbol:
                {
                    return new EntityTypeModel
                    {
                        Name = fieldSymbol.Type.ToDisplayString()
                    };
                }
                case IMethodSymbol methodSymbol:
                    if (methodSymbol.MethodKind == MethodKind.Constructor && methodSymbol.ReceiverType != null)
                    {
                        return new EntityTypeModel
                        {
                            Name = methodSymbol.ReceiverType.ToDisplayString()
                        };
                    }

                    return new EntityTypeModel
                    {
                        Name = methodSymbol.ReturnType.ToDisplayString()
                    };
                case IParameterSymbol parameterSymbol:
                {
                    return new EntityTypeModel
                    {
                        Name = parameterSymbol.Type.ToDisplayString()
                    };
                }
                default:
                {
                    return new EntityTypeModel
                    {
                        Name = symbolInfo.ToString()
                    };
                }
            }
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
