using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class AccessFieldVisitor : IExtractionVisitor<ExpressionSyntax, SemanticModel, AccessedField>
{
    public AccessedField Visit(ExpressionSyntax? syntaxNode, SemanticModel semanticModel, AccessedField modelType)
    {
        if (syntaxNode == null)
        {
            return modelType;
        }

        var symbolInfo = semanticModel.GetSymbolInfo(syntaxNode);

        if (symbolInfo.Symbol is IFieldSymbol or IPropertySymbol)
        {
            return new AccessedField
            {
                Name = symbolInfo.Symbol.Name,
                DefinitionClassName =
                    GetDefinitionClassName(syntaxNode, semanticModel),
                LocationClassName = GetLocationClassName(syntaxNode, semanticModel),
                Kind = GetAccessType(syntaxNode),
            };
        }

        if (syntaxNode is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
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
        return syntax?.Parent is AssignmentStatementSyntax
            ? AccessedField.AccessKind.Setter
            : AccessedField.AccessKind.Getter;
    }
}
