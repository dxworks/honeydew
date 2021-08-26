using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction
{
    public class CSharpExtractionHelperMethods : IExtractionHelperMethods
    {
        private readonly SemanticModel _semanticModel;

        public CSharpExtractionHelperMethods(CSharpSemanticModel semanticModel)
        {
            _semanticModel = semanticModel.Model;
        }

        public string GetFullName(MemberDeclarationSyntax declarationSyntax)
        {
            var declaredSymbol = _semanticModel.GetDeclaredSymbol(declarationSyntax);
            if (declaredSymbol != null)
            {
                return declaredSymbol.ToDisplayString();
            }

            return declarationSyntax switch
            {
                DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier.ToString(),
                BaseTypeDeclarationSyntax baseTypeDeclarationSyntax => baseTypeDeclarationSyntax.Identifier.ToString(),
                _ => declarationSyntax.ToString()
            };
        }

        public IEntityType GetFullName(VariableDeclarationSyntax declarationSyntax)
        {
            var typeName = declarationSyntax.Type.ToString();
            var nodeSymbol = ModelExtensions.GetSymbolInfo(_semanticModel, declarationSyntax.Type).Symbol;
            if (nodeSymbol != null)
            {
                typeName = nodeSymbol.ToString();
            }

            return new EntityTypeModel
            {
                Name = typeName
            };
        }

        public IEntityType GetFullName(PropertyDeclarationSyntax declarationSyntax)
        {
            var typeName = declarationSyntax.Type.ToString();
            var nodeSymbol = ModelExtensions.GetSymbolInfo(_semanticModel, declarationSyntax.Type).Symbol;
            if (nodeSymbol != null)
            {
                typeName = nodeSymbol.ToString();
            }

            return new EntityTypeModel
            {
                Name = typeName
            };
        }

        public IEntityType GetFullName(TypeSyntax typeSyntax)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(typeSyntax);
            if (symbolInfo.Symbol != null)
            {
                return new EntityTypeModel
                {
                    Name = symbolInfo.Symbol.ToString()
                };
            }

            if (typeSyntax is RefTypeSyntax refTypeSyntax)
            {
                return GetFullName(refTypeSyntax.Type);
            }

            if (typeSyntax is ArrayTypeSyntax arrayTypeSyntax)
            {
                return new EntityTypeModel
                {
                    Name = $"{GetFullName(arrayTypeSyntax.ElementType).Name}{arrayTypeSyntax.RankSpecifiers.ToString()}"
                };
            }

            return new EntityTypeModel
            {
                Name = typeSyntax.ToString()
            };
        }

        public IEntityType GetFullName(BaseObjectCreationExpressionSyntax declarationSyntax)
        {
            var symbolInfo = ModelExtensions.GetSymbolInfo(_semanticModel, declarationSyntax);

            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                return new EntityTypeModel
                {
                    Name = methodSymbol.ContainingType.ToDisplayString()
                };
            }

            var variableDeclarationSyntax = GetParentDeclarationSyntax<VariableDeclarationSyntax>(declarationSyntax);
            if (variableDeclarationSyntax != null)
            {
                return GetFullName(variableDeclarationSyntax);
            }

            var propertyDeclarationSyntax = GetParentDeclarationSyntax<PropertyDeclarationSyntax>(declarationSyntax);
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


        public IEntityType GetFullName(ImplicitArrayCreationExpressionSyntax declarationSyntax)
        {
            var propertyDeclarationSyntax = GetParentDeclarationSyntax<PropertyDeclarationSyntax>(declarationSyntax);
            if (propertyDeclarationSyntax != null)
            {
                return GetFullName(propertyDeclarationSyntax.Type);
            }

            var variableDeclarationSyntax = GetParentDeclarationSyntax<VariableDeclarationSyntax>(declarationSyntax);
            if (variableDeclarationSyntax != null)
            {
                return GetFullName(variableDeclarationSyntax);
            }

            // try to infer type from elements
            var elementTypesSet = new HashSet<string>();

            foreach (var expression in declarationSyntax.Initializer.Expressions)
            {
                var fullName = GetExpressionType(expression).Name;
                fullName = CSharpConstants.ConvertPrimitiveTypeToSystemType(fullName);
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

        public IEntityType GetFullName(ExpressionSyntax expressionSyntax)
        {
            if (expressionSyntax == null)
            {
                return "";
            }

            var symbolInfo = _semanticModel.GetSymbolInfo(expressionSyntax);

            switch (symbolInfo.Symbol)
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
                    if (expressionSyntax is ObjectCreationExpressionSyntax && methodSymbol.ReceiverType != null)
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
                default:
                {
                    if (symbolInfo.Symbol == null)
                    {
                        return expressionSyntax switch
                        {
                            ObjectCreationExpressionSyntax objectCreationExpressionSyntax => GetFullName(
                                objectCreationExpressionSyntax),
                            _ => new EntityTypeModel
                            {
                                Name = expressionSyntax.ToString()
                            }
                        };
                    }

                    return new EntityTypeModel
                    {
                        Name = symbolInfo.Symbol.ToString()
                    };
                }
            }
        }

        public IEntityType GetFullName(ThrowStatementSyntax declarationSyntax)
        {
            var parentDeclarationSyntax = GetParentDeclarationSyntax<CatchClauseSyntax>(declarationSyntax);
            var catchDeclarationSyntax = parentDeclarationSyntax.Declaration;
            if (catchDeclarationSyntax != null)
            {
                return GetFullName(catchDeclarationSyntax.Type);
            }

            return GetFullName(declarationSyntax.Expression);
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

        private static T GetParentDeclarationSyntax<T>(SyntaxNode node) where T : SyntaxNode
        {
            var rootNode = node;
            while (true)
            {
                if (node is T syntax && node != rootNode)
                {
                    return syntax;
                }

                if (node.Parent == null)
                {
                    return null;
                }

                node = node.Parent;
            }
        }

        public IList<IEntityType> GetBaseInterfaces(TypeDeclarationSyntax node)
        {
            var declaredSymbol = ModelExtensions.GetDeclaredSymbol(_semanticModel, node);

            IList<IEntityType> interfaces = new List<IEntityType>();

            if (declaredSymbol is not ITypeSymbol typeSymbol)
            {
                return interfaces;
            }

            foreach (var interfaceSymbol in typeSymbol.Interfaces)
            {
                interfaces.Add(new EntityTypeModel
                {
                    Name = interfaceSymbol.ToString()
                });
            }

            return interfaces;
        }

        public IEntityType GetBaseClassName(TypeDeclarationSyntax node)
        {
            var declaredSymbol = ModelExtensions.GetDeclaredSymbol(_semanticModel, node);

            if (declaredSymbol is not ITypeSymbol typeSymbol)
            {
                return new EntityTypeModel
                {
                    Name = CSharpConstants.ObjectIdentifier
                };
            }

            if (typeSymbol.BaseType == null)
            {
                return new EntityTypeModel
                {
                    Name = CSharpConstants.ObjectIdentifier
                };
            }

            return new EntityTypeModel
            {
                Name = typeSymbol.BaseType.ToString()
            };
        }

        public IEntityType GetBaseClassName(BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
        {
            if (baseTypeDeclarationSyntax is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                return GetBaseClassName(typeDeclarationSyntax);
            }

            return new EntityTypeModel
            {
                Name = CSharpConstants.ObjectIdentifier
            };
        }

        public IMethodSymbol GetMethodSymbol(CSharpSyntaxNode expressionSyntax)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(expressionSyntax);
            var symbol = symbolInfo.Symbol;
            if (symbol is IMethodSymbol methodSymbol)
            {
                return methodSymbol;
            }

            return null;
        }

        public MethodCallModel GetMethodCallModel(InvocationExpressionSyntax invocationExpressionSyntax)
        {
            var calledMethodName = invocationExpressionSyntax.Expression.ToString();

            string containingClassName = null;

            var parentLocalFunctionSyntax =
                GetParentDeclarationSyntax<LocalFunctionStatementSyntax>(invocationExpressionSyntax);

            if (parentLocalFunctionSyntax == null)
            {
                var symbolInfo = ModelExtensions.GetSymbolInfo(_semanticModel, invocationExpressionSyntax);
                if (symbolInfo.Symbol != null)
                {
                    containingClassName = symbolInfo.Symbol.ContainingType.ToString();
                }
            }
            else
            {
                while (parentLocalFunctionSyntax != null)
                {
                    if (parentLocalFunctionSyntax.Body != null)
                    {
                        var localFunctionStatementSyntax = parentLocalFunctionSyntax.Body.ChildNodes()
                            .OfType<LocalFunctionStatementSyntax>()
                            .SingleOrDefault(childNode => childNode.Identifier.ToString() == calledMethodName);
                        if (localFunctionStatementSyntax != null)
                        {
                            parentLocalFunctionSyntax = localFunctionStatementSyntax;
                            break;
                        }
                    }

                    parentLocalFunctionSyntax =
                        GetParentDeclarationSyntax<LocalFunctionStatementSyntax>(parentLocalFunctionSyntax.Parent);
                }

                if (parentLocalFunctionSyntax != null)
                {
                    containingClassName = GetParentDeclaredType(parentLocalFunctionSyntax);
                }
                else
                {
                    containingClassName = GetParentDeclaredType(invocationExpressionSyntax);
                }
            }

            switch (invocationExpressionSyntax.Expression)
            {
                case IdentifierNameSyntax:
                    return new MethodCallModel
                    {
                        Name = calledMethodName,
                        ContainingTypeName = containingClassName,
                        ParameterTypes = GetParameters(invocationExpressionSyntax)
                    };

                case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                {
                    var className = containingClassName;

                    if (memberAccessExpressionSyntax.Expression.ToFullString() != CSharpConstants.BaseClassIdentifier)
                    {
                        className = GetFullName(memberAccessExpressionSyntax.Expression).Name ??
                                    containingClassName;
                    }

                    return new MethodCallModel
                    {
                        Name = memberAccessExpressionSyntax.Name.ToString(),
                        ContainingTypeName = className,
                        ParameterTypes = GetParameters(invocationExpressionSyntax)
                    };
                }
            }

            return new MethodCallModel
            {
                Name = calledMethodName,
                ContainingTypeName = containingClassName,
                ParameterTypes = GetParameters(invocationExpressionSyntax)
            };
        }

        public IList<IParameterType> GetParameters(IMethodSymbol methodSymbol)
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
                    defaultValue = parameter.ExplicitDefaultValue?.ToString();
                }

                parameters.Add(new ParameterModel
                {
                    Type = new EntityTypeModel
                    {
                        Name = parameter.Type.ToString()
                    },
                    Modifier = modifier,
                    DefaultValue = defaultValue
                });
            }

            return parameters;
        }

        public string GetAliasTypeOfNamespace(NameSyntax nodeName)
        {
            var symbolInfo = ModelExtensions.GetSymbolInfo(_semanticModel, nodeName);
            return symbolInfo.Symbol switch
            {
                null => nameof(EAliasType.NotDetermined),
                INamespaceSymbol => nameof(EAliasType.Namespace),
                _ => nameof(EAliasType.Class)
            };
        }
        
        public ParameterModel ExtractInfoAboutParameter(BaseParameterSyntax baseParameterSyntax)
        {
            var parameterType = GetFullName(baseParameterSyntax.Type);

            string defaultValue = null;

            if (baseParameterSyntax is ParameterSyntax parameterSyntax)
            {
                defaultValue = parameterSyntax.Default?.Value.ToString();
            }

            return new ParameterModel
            {
                Type = parameterType,
                Modifier = baseParameterSyntax.Modifiers.ToString(),
                DefaultValue = defaultValue
            };
        }

        public string GetParentDeclaredType(SyntaxNode syntaxNode)
        {
            CSharpSyntaxNode declarationSyntax = GetParentDeclarationSyntax<LocalFunctionStatementSyntax>(syntaxNode);

            if (declarationSyntax == null)
            {
                declarationSyntax = GetParentDeclarationSyntax<BaseMethodDeclarationSyntax>(syntaxNode);
            }

            if (declarationSyntax == null)
            {
                declarationSyntax = GetParentDeclarationSyntax<PropertyDeclarationSyntax>(syntaxNode);
            }

            if (declarationSyntax == null)
            {
                declarationSyntax = GetParentDeclarationSyntax<EventDeclarationSyntax>(syntaxNode);
            }

            if (declarationSyntax != null)
            {
                var declaredSymbol = _semanticModel.GetDeclaredSymbol(declarationSyntax);
                if (declaredSymbol == null)
                {
                    return syntaxNode.ToString();
                }

                if (declarationSyntax is LocalFunctionStatementSyntax)
                {
                    var parent = GetParentDeclaredType(declarationSyntax.Parent);
                    return $"{parent}.{declaredSymbol}";
                }

                return declaredSymbol.ToString();
            }

            return syntaxNode.ToString();
        }

        public IEntityType GetContainingType(SyntaxNode syntaxNode)
        {
            var variableDeclarationSyntax = GetParentDeclarationSyntax<VariableDeclarationSyntax>(syntaxNode);
            if (variableDeclarationSyntax != null)
            {
                var fullName = GetFullName(variableDeclarationSyntax);
                if (fullName.Name != CSharpConstants.VarIdentifier)
                {
                    return fullName;
                }
            }

            var propertyDeclarationSyntax = GetParentDeclarationSyntax<PropertyDeclarationSyntax>(syntaxNode);
            if (propertyDeclarationSyntax != null)
            {
                return GetFullName(propertyDeclarationSyntax);
            }

            return new EntityTypeModel
            {
                Name = syntaxNode.ToString()
            };
        }

        private IList<IParameterType> GetParameters(InvocationExpressionSyntax invocationSyntax)
        {
            var methodSymbol = GetMethodSymbol(invocationSyntax);
            if (methodSymbol == null)
            {
                // try to reconstruct the parameters from the method call
                var parameterList = new List<IParameterType>();
                var success = true;

                foreach (var argumentSyntax in invocationSyntax.ArgumentList.Arguments)
                {
                    var parameterSymbolInfo = ModelExtensions.GetSymbolInfo(_semanticModel, argumentSyntax.Expression);
                    if (parameterSymbolInfo.Symbol != null)
                    {
                        parameterList.Add(new ParameterModel
                        {
                            Type = GetFullName(argumentSyntax.Expression)
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
                        Type = new EntityTypeModel
                        {
                            Name = literalExpressionSyntax.Token.Value.GetType().FullName
                        }
                    });
                }

                return success ? parameterList : new List<IParameterType>();
            }

            return GetParameters(methodSymbol);
        }


        // syntactic
        public string SetTypeModifier(string typeString, string modifier)
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

        public int CalculateCyclomaticComplexity(MemberDeclarationSyntax syntax)
        {
            return CalculateCyclomaticComplexityForSyntaxNode(syntax) + 1;
        }

        public int CalculateCyclomaticComplexity(LocalFunctionStatementSyntax syntax)
        {
            return CalculateCyclomaticComplexityForSyntaxNode(syntax) + 1;
        }

        public bool IsAbstractModifier(string modifier)
        {
            return CSharpConstants.AbstractIdentifier == modifier;
        }

        private int CalculateCyclomaticComplexity(ExpressionSyntax conditionExpressionSyntax)
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

        private int CalculateCyclomaticComplexityForSyntaxNode(SyntaxNode syntax)
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

        public string GetFullName(AttributeSyntax attributeSyntax)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(attributeSyntax);
            if (symbolInfo.Symbol != null)
            {
                return symbolInfo.Symbol.ContainingType.ToString();
            }

            return attributeSyntax.Name.ToString();
        }

        public string GetAttributeContainingType(AttributeSyntax syntaxNode)
        {
            var baseMethodDeclarationSyntax = GetParentDeclarationSyntax<BaseMethodDeclarationSyntax>(syntaxNode);
            if (baseMethodDeclarationSyntax != null)
            {
                return GetFullName(baseMethodDeclarationSyntax);
            }

            var delegateDeclarationSyntax = GetParentDeclarationSyntax<DelegateDeclarationSyntax>(syntaxNode);
            if (delegateDeclarationSyntax != null)
            {
                return GetFullName(delegateDeclarationSyntax);
            }

            var parentDeclarationSyntax = GetParentDeclarationSyntax<BaseTypeDeclarationSyntax>(syntaxNode);
            if (parentDeclarationSyntax != null)
            {
                return GetFullName(parentDeclarationSyntax);
            }

            return syntaxNode.ToString();
        }

        public IEnumerable<IParameterType> GetParameters(AttributeSyntax attributeSyntax)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(attributeSyntax);
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
                    var parameterSymbolInfo = _semanticModel.GetSymbolInfo(argumentSyntax.Expression);
                    var parameterType = CSharpConstants.SystemObject;
                    if (parameterSymbolInfo.Symbol != null)
                    {
                        parameterTypes.Add(new ParameterModel
                        {
                            Type = GetFullName(argumentSyntax.Expression)
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
                        Type = new EntityTypeModel
                        {
                            Name = parameterType
                        }
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

        public string GetAttributeTarget(AttributeSyntax syntaxNode)
        {
            var attributeListSyntax = GetParentDeclarationSyntax<AttributeListSyntax>(syntaxNode);

            if (attributeListSyntax?.Target == null)
            {
                return "";
            }

            return attributeListSyntax.Target.Identifier.ToString();
        }
    }
}
