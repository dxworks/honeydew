using Honeydew.Extractors.Dotnet;
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
        if (syntaxNode is null)
        {
            return modelType;
        }

        if (syntaxNode is ElementAccessExpressionSyntax elementAccessExpressionSyntax)
        {
            syntaxNode = elementAccessExpressionSyntax.Expression;
        }

        var accessedField = ExtractAccessedFieldWithSemanticModel(syntaxNode, semanticModel);
        if (accessedField is not null)
        {
            return accessedField;
        }

        accessedField = ExtractAccessedFieldWithoutSemanticModel(syntaxNode, semanticModel);
        if (accessedField is not null)
        {
            return accessedField;
        }

        return modelType;
    }

    private static AccessedField? ExtractAccessedFieldWithSemanticModel(SyntaxNode syntaxNode,
        SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(syntaxNode);

        if (symbolInfo.Symbol is IFieldSymbol or IPropertySymbol)
        {
            return new AccessedField
            {
                Name = symbolInfo.Symbol.Name,
                DefinitionClassName = GetDefinitionClassName(syntaxNode, semanticModel),
                LocationClassName = GetLocationClassName(syntaxNode, semanticModel),
                Kind = GetAccessType(syntaxNode),
            };
        }

        return null;
    }

    private static AccessedField? ExtractAccessedFieldWithoutSemanticModel(SyntaxNode syntaxNode,
        SemanticModel semanticModel)
    {
        if (syntaxNode.Parent is MemberAccessExpressionSyntax memberAccessExpressionSyntax &&
            memberAccessExpressionSyntax.Name == syntaxNode)
        {
            if (memberAccessExpressionSyntax.Parent is InvocationExpressionSyntax)
            {
                return null;
            }

            var definitionClassName = GetDefinitionClassName(memberAccessExpressionSyntax, semanticModel);
            return new AccessedField
            {
                Name = memberAccessExpressionSyntax.Name.ToString(),
                DefinitionClassName = definitionClassName,
                LocationClassName = definitionClassName,
                Kind = GetAccessType(memberAccessExpressionSyntax),
            };
        }

        return null;
    }


    private static AccessedField.AccessKind GetAccessType(SyntaxNode? syntax)
    {
        if (syntax is null)
        {
            return AccessedField.AccessKind.Getter;
        }

        if (syntax.Parent is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            if (memberAccessExpressionSyntax.Expression == syntax)
            {
                return AccessedField.AccessKind.Getter;
            }
        }

        var assignmentExpressionSyntax = syntax.GetParentDeclarationSyntax<AssignmentExpressionSyntax>();

        if (assignmentExpressionSyntax is null)
        {
            return AccessedField.AccessKind.Getter;
        }

        if (assignmentExpressionSyntax.Right.DescendantNodes().Any(s => s == syntax))
        {
            return AccessedField.AccessKind.Getter;
        }

        return AccessedField.AccessKind.Setter;
    }
}
