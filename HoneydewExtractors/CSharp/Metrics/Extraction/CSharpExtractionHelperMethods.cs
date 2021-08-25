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

        public string GetFullName(VariableDeclarationSyntax declarationSyntax)
        {
            var typeName = declarationSyntax.Type.ToString();
            var nodeSymbol = ModelExtensions.GetSymbolInfo(_semanticModel, declarationSyntax.Type).Symbol;
            if (nodeSymbol != null)
            {
                typeName = nodeSymbol.ToString();
            }

            return typeName;
        }

        public string GetFullName(PropertyDeclarationSyntax declarationSyntax)
        {
            var typeName = declarationSyntax.Type.ToString();
            var nodeSymbol = ModelExtensions.GetSymbolInfo(_semanticModel, declarationSyntax.Type).Symbol;
            if (nodeSymbol != null)
            {
                typeName = nodeSymbol.ToString();
            }

            return typeName;
        }

        public string GetFullName(TypeSyntax typeSyntax)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(typeSyntax);
            if (symbolInfo.Symbol != null)
            {
                return symbolInfo.Symbol.ToString();
            }

            if (typeSyntax is RefTypeSyntax refTypeSyntax)
            {
                return GetFullName(refTypeSyntax.Type);
            }

            if (typeSyntax is ArrayTypeSyntax arrayTypeSyntax)
            {
                return $"{GetFullName(arrayTypeSyntax.ElementType)}{arrayTypeSyntax.RankSpecifiers.ToString()}";
            }

            return typeSyntax.ToString();
        }

        public string GetFullName(BaseObjectCreationExpressionSyntax declarationSyntax)
        {
            var symbolInfo = ModelExtensions.GetSymbolInfo(_semanticModel, declarationSyntax);

            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                return methodSymbol.ContainingType.ToDisplayString();
            }

            var variableDeclarationSyntax = GetParentDeclarationSyntax<VariableDeclarationSyntax>(declarationSyntax);
            if (variableDeclarationSyntax != null)
            {
                return GetFullName(variableDeclarationSyntax);
            }

            var propertyDeclarationSyntax = GetParentDeclarationSyntax<PropertyDeclarationSyntax>(declarationSyntax);
            if (propertyDeclarationSyntax != null)
            {
                return propertyDeclarationSyntax.Type.ToString();
            }

            if (declarationSyntax is ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
            {
                return GetFullName(objectCreationExpressionSyntax.Type);
            }

            return declarationSyntax.ToString();
        }


        public string GetFullName(ImplicitArrayCreationExpressionSyntax declarationSyntax)
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
                var fullName = GetExpressionType(expression);
                fullName = CSharpConstants.ConvertPrimitiveTypeToSystemType(fullName);
                elementTypesSet.Add(fullName);
            }

            switch (elementTypesSet.Count)
            {
                case 0:
                    return declarationSyntax.ToString();
                case 1:
                    return $"{elementTypesSet.First()}[]";
                case 2:
                {
                    if (elementTypesSet.Contains("System.Int32") && elementTypesSet.Contains("System.Single"))
                    {
                        return "System.Single[]";
                    }

                    if (elementTypesSet.Contains("System.Int32") && elementTypesSet.Contains("System.Double"))
                    {
                        return "System.Double[]";
                    }

                    if (elementTypesSet.Contains("System.Single") && elementTypesSet.Contains("System.Double"))
                    {
                        return "System.Double[]";
                    }

                    return "System.Object[]";
                }
                case 3:
                {
                    if (elementTypesSet.Contains("System.Int32") && elementTypesSet.Contains("System.Single") &&
                        elementTypesSet.Contains("System.Double"))
                    {
                        return "System.Double[]";
                    }

                    return "System.Object[]";
                }
                default:
                    return "System.Object[]";
            }
        }

        public string GetFullName(ExpressionSyntax expressionSyntax)
        {
            if (expressionSyntax == null)
            {
                return "";
            }

            var symbolInfo = _semanticModel.GetSymbolInfo(expressionSyntax);

            switch (symbolInfo.Symbol)
            {
                case IPropertySymbol propertySymbol:
                    return propertySymbol.Type.ToDisplayString();
                case ILocalSymbol localSymbol:
                    return localSymbol.Type.ToDisplayString();
                case IFieldSymbol fieldSymbol:
                    return fieldSymbol.Type.ToDisplayString();
                case IMethodSymbol methodSymbol:
                    if (expressionSyntax is ObjectCreationExpressionSyntax && methodSymbol.ReceiverType != null)
                    {
                        return methodSymbol.ReceiverType.ToDisplayString();
                    }

                    return methodSymbol.ReturnType.ToDisplayString();
                default:
                {
                    if (symbolInfo.Symbol == null)
                    {
                        return expressionSyntax switch
                        {
                            ObjectCreationExpressionSyntax objectCreationExpressionSyntax => GetFullName(
                                objectCreationExpressionSyntax),
                            _ => expressionSyntax.ToString()
                        };
                    }

                    return symbolInfo.Symbol.ToString();
                }
            }
        }

        public string GetFullName(ThrowStatementSyntax declarationSyntax)
        {
            var parentDeclarationSyntax = GetParentDeclarationSyntax<CatchClauseSyntax>(declarationSyntax);
            var catchDeclarationSyntax = parentDeclarationSyntax.Declaration;
            if (catchDeclarationSyntax != null)
            {
                return GetFullName(catchDeclarationSyntax.Type);
            }

            return GetFullName(declarationSyntax.Expression);
        }

        private string GetExpressionType(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literalExpressionSyntax:
                {
                    if (literalExpressionSyntax.Token.Value != null)
                    {
                        return literalExpressionSyntax.Token.Value.GetType().FullName;
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

        public IList<string> GetBaseInterfaces(TypeDeclarationSyntax node)
        {
            var declaredSymbol = ModelExtensions.GetDeclaredSymbol(_semanticModel, node);

            IList<string> interfaces = new List<string>();

            if (declaredSymbol is not ITypeSymbol typeSymbol)
            {
                return interfaces;
            }

            foreach (var interfaceSymbol in typeSymbol.Interfaces)
            {
                interfaces.Add(interfaceSymbol.ToString());
            }

            return interfaces;
        }

        public string GetBaseClassName(TypeDeclarationSyntax node)
        {
            var declaredSymbol = ModelExtensions.GetDeclaredSymbol(_semanticModel, node);

            if (declaredSymbol is not ITypeSymbol typeSymbol)
            {
                return CSharpConstants.ObjectIdentifier;
            }

            if (typeSymbol.BaseType == null)
            {
                return CSharpConstants.ObjectIdentifier;
            }

            return typeSymbol.BaseType.ToString();
        }

        public string GetBaseClassName(BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
        {
            if (baseTypeDeclarationSyntax is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                return GetBaseClassName(typeDeclarationSyntax);
            }

            return CSharpConstants.ObjectIdentifier;
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
                        className = GetFullName(memberAccessExpressionSyntax.Expression) ??
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
                    Name = parameter.Type.ToString(),
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

        public IList<IParameterType> ExtractInfoAboutParameters(BaseParameterListSyntax parameterList)
        {
            var parameterTypes = new List<IParameterType>();

            foreach (var parameter in parameterList.Parameters)
            {
                var parameterType = GetFullName(parameter.Type);


                parameterTypes.Add(new ParameterModel
                {
                    Name = parameterType,
                    Modifier = parameter.Modifiers.ToString(),
                    DefaultValue = parameter.Default?.Value.ToString()
                });
            }

            return parameterTypes;
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

        public string GetContainingType(SyntaxNode syntaxNode)
        {
            var variableDeclarationSyntax = GetParentDeclarationSyntax<VariableDeclarationSyntax>(syntaxNode);
            if (variableDeclarationSyntax != null)
            {
                var fullName = GetFullName(variableDeclarationSyntax);
                if (fullName != CSharpConstants.VarIdentifier)
                {
                    return fullName;
                }
            }

            var propertyDeclarationSyntax = GetParentDeclarationSyntax<PropertyDeclarationSyntax>(syntaxNode);
            if (propertyDeclarationSyntax != null)
            {
                return GetFullName(propertyDeclarationSyntax);
            }

            return syntaxNode.ToString();
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
                            Name = GetFullName(argumentSyntax.Expression)
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
                        Name = literalExpressionSyntax.Token.Value.GetType().FullName
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
    }
}
