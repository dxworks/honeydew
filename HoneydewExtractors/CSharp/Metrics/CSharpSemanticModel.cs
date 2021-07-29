using System.Collections.Generic;
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

        public string GetFullName(TypeSyntax typeSyntax)
        {
            var returnValueSymbol = Model.GetSymbolInfo(typeSyntax);
            return returnValueSymbol.Symbol != null
                ? returnValueSymbol.Symbol.ToString()
                : typeSyntax.ToString();
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
            string containingClassName, string baseTypeName = "object")
        {
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

        private IList<ParameterModel> GetParameterTypes(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode syntaxNode)
        {
            var methodSymbol = GetMethodSymbol(syntaxNode);
            return methodSymbol == null ? new List<ParameterModel>() : GetParameterTypes(methodSymbol);
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

        public bool IsNamespace(NameSyntax nodeName)
        {
            var symbolInfo = Model.GetSymbolInfo(nodeName);
            switch (symbolInfo.Symbol)
            {
                case null:
                case INamespaceSymbol:
                    return true;
                default:
                    return false;
            }
        }
    }
}
