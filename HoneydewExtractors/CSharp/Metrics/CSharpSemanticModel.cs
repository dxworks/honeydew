using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpSemanticModel : ISemanticModel
    {
        public SemanticModel Model { private get; init; }

        public string GetFullName(MemberDeclarationSyntax declarationSyntax)
        {
            var declaredSymbol = Model.GetDeclaredSymbol(declarationSyntax);
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
            var nodeSymbol = Model.GetSymbolInfo(declarationSyntax.Type).Symbol;
            if (nodeSymbol != null)
            {
                typeName = nodeSymbol.ToString();
            }

            return typeName;
        }

        public string GetFullName(PropertyDeclarationSyntax declarationSyntax)
        {
            var typeName = declarationSyntax.Type.ToString();
            var nodeSymbol = Model.GetSymbolInfo(declarationSyntax.Type).Symbol;
            if (nodeSymbol != null)
            {
                typeName = nodeSymbol.ToString();
            }

            return typeName;
        }

        public string GetFullName(TypeSyntax typeSyntax)
        {
            var symbolInfo = Model.GetSymbolInfo(typeSyntax);
            if (symbolInfo.Symbol != null)
            {
                return symbolInfo.Symbol.ToString();
            }

            var variableDeclarationSyntax = GetParentDeclarationSyntax<VariableDeclarationSyntax>(typeSyntax);
            if (variableDeclarationSyntax != null)
            {
                var fullName = GetFullName(variableDeclarationSyntax);
                if (fullName != CSharpConstants.VarIdentifier)
                {
                    return fullName;
                }
            }

            var propertyDeclarationSyntax = GetParentDeclarationSyntax<PropertyDeclarationSyntax>(typeSyntax);
            if (propertyDeclarationSyntax != null)
            {
                return GetFullName(propertyDeclarationSyntax);
            }

            return typeSyntax.ToString();
        }

        public string GetFullName(ImplicitObjectCreationExpressionSyntax declarationSyntax)
        {
            var symbolInfo = Model.GetSymbolInfo(declarationSyntax);

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
                    if (elementTypesSet.Contains("System.Int32") && elementTypesSet.Contains("System.Single") && elementTypesSet.Contains("System.Double"))
                    {
                        return "System.Double[]";
                    }

                    return "System.Object[]";
                }
                default:
                    return "System.Object[]";
            }
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
            while (true)
            {
                if (node is T syntax)
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
            var declaredSymbol = Model.GetDeclaredSymbol(node);

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
            var declaredSymbol = Model.GetDeclaredSymbol(node);

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

        public IMethodSymbol GetMethodSymbol(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode expressionSyntax)
        {
            var symbolInfo = Model.GetSymbolInfo(expressionSyntax);
            var symbol = symbolInfo.Symbol;
            if (symbol is IMethodSymbol methodSymbol)
            {
                return methodSymbol;
            }

            return null;
        }

        public MethodCallModel GetMethodCallModel(InvocationExpressionSyntax invocationExpressionSyntax,
            string baseTypeName = "object")
        {
            string containingClassName = null;

            var symbolInfo = Model.GetSymbolInfo(invocationExpressionSyntax);
            if (symbolInfo.Symbol != null)
            {
                containingClassName = symbolInfo.Symbol.ContainingType.ToString();
            }

            switch (invocationExpressionSyntax.Expression)
            {
                case IdentifierNameSyntax:
                    return new MethodCallModel
                    {
                        MethodName = invocationExpressionSyntax.Expression.ToString(),
                        ContainingClassName = containingClassName,
                        ParameterTypes = GetParameterTypes(invocationExpressionSyntax)
                    };

                case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                {
                    string className;

                    if (memberAccessExpressionSyntax.Expression.ToFullString() ==
                        CSharpConstants.BaseClassIdentifier)
                    {
                        className = baseTypeName;
                    }
                    else
                    {
                        className = GetFullName(memberAccessExpressionSyntax.Expression) ??
                                    containingClassName;
                    }

                    return new MethodCallModel
                    {
                        MethodName = memberAccessExpressionSyntax.Name.ToString(),
                        ContainingClassName = className,
                        ParameterTypes = GetParameterTypes(invocationExpressionSyntax)
                    };
                }
            }

            return null;
        }

        public IList<ParameterModel> GetParameterTypes(IMethodSymbol methodSymbol)
        {
            IList<ParameterModel> parameterTypes = new List<ParameterModel>();
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

                parameterTypes.Add(new ParameterModel
                {
                    Type = parameter.Type.ToString(),
                    Modifier = modifier,
                    DefaultValue = defaultValue
                });
            }

            return parameterTypes;
        }

        public EAliasType GetAliasTypeOfNamespace(NameSyntax nodeName)
        {
            var symbolInfo = Model.GetSymbolInfo(nodeName);
            return symbolInfo.Symbol switch
            {
                null => EAliasType.NotDetermined,
                INamespaceSymbol => EAliasType.Namespace,
                _ => EAliasType.Class
            };
        }

        private IList<ParameterModel> GetParameterTypes(InvocationExpressionSyntax invocationSyntax)
        {
            var methodSymbol = GetMethodSymbol(invocationSyntax);
            if (methodSymbol == null)
            {
                // try to reconstruct the parameters from the method call
                var parameterList = new List<ParameterModel>();
                var success = true;

                foreach (var argumentSyntax in invocationSyntax.ArgumentList.Arguments)
                {
                    var parameterSymbolInfo = Model.GetSymbolInfo(argumentSyntax.Expression);
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
                        Type = literalExpressionSyntax.Token.Value.GetType().FullName
                    });
                }

                return success ? parameterList : new List<ParameterModel>();
            }

            return GetParameterTypes(methodSymbol);
        }

        private string GetFullName(ExpressionSyntax expressionSyntax)
        {
            var symbolInfo = Model.GetSymbolInfo(expressionSyntax);
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
                        return expressionSyntax.ToString();
                    }

                    return symbolInfo.Symbol.ToString();
                }
            }
        }
    }
}
