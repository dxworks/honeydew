using System.Collections.Generic;
using HoneydewExtractors.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Metrics.CSharp
{
    public class CSharpSemanticModel : ISemanticModel
    {
        public SemanticModel Model { private get; init; }

        public string GetFullName(BaseTypeDeclarationSyntax declarationSyntax)
        {
            var declaredSymbol = Model.GetDeclaredSymbol(declarationSyntax);
            if (declaredSymbol == null)
            {
                return declarationSyntax.Identifier.ToString();
            }

            return declaredSymbol.ToDisplayString();
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

        public string GetFullName(ITypeSymbol typeSyntax)
        {
            return typeSyntax.ToString();
        }

        public string GetFullName(TypeSyntax typeSyntax)
        {
            var returnValueSymbol = Model.GetSymbolInfo(typeSyntax);
            return returnValueSymbol.Symbol != null
                ? returnValueSymbol.Symbol.ToString()
                : typeSyntax.ToString();
        }

        public string GetFullName(ExpressionSyntax expressionSyntax)
        {
            var symbolInfo = Model.GetSymbolInfo(expressionSyntax);
            if (symbolInfo.Symbol is ILocalSymbol localSymbol)
            {
                return localSymbol.Type.ToDisplayString();
            }

            return symbolInfo.Symbol?.ToString();
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
    }
}
