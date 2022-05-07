using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class AccessFieldVisitor : IExtractionVisitor<ExpressionSyntax, SemanticModel, AccessedField>
{
    public AccessedField Visit(ExpressionSyntax? syntaxNode, SemanticModel semanticModel, AccessedField modelType)
    {
        var expressionSyntax = syntaxNode;
        if (expressionSyntax == null)
        {
            return modelType;
        }

        if (syntaxNode is ElementAccessExpressionSyntax elementAccessExpressionSyntax)
        {
            expressionSyntax = elementAccessExpressionSyntax.Expression;
        }

        var symbolInfo = semanticModel.GetSymbolInfo(expressionSyntax);

        if (symbolInfo.Symbol is IFieldSymbol or IPropertySymbol)
        {
            return new AccessedField
            {
                Name = symbolInfo.Symbol.Name,
                DefinitionClassName =
                    GetDefinitionClassName(expressionSyntax, semanticModel),
                LocationClassName = GetLocationClassName(expressionSyntax, semanticModel),
                Kind = GetAccessType(syntaxNode),
            };
        }

        if (expressionSyntax is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            if (memberAccessExpressionSyntax.Parent is InvocationExpressionSyntax)
            {
                return modelType;
            }

            var definitionClassName =
                GetDefinitionClassName(memberAccessExpressionSyntax, semanticModel);
            return new AccessedField
            {
                Name = memberAccessExpressionSyntax.Name.ToString(),
                DefinitionClassName = definitionClassName,
                LocationClassName = definitionClassName,
                Kind = GetAccessType(memberAccessExpressionSyntax),
            };
        }

        return modelType;
    }


    private static AccessedField.AccessKind GetAccessType(SyntaxNode? syntax)
    {
        return syntax?.Parent is AssignmentExpressionSyntax
            ? AccessedField.AccessKind.Setter
            : AccessedField.AccessKind.Getter;
    }
}
