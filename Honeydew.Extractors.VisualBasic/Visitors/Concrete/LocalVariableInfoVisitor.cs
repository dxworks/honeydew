using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class LocalVariableInfoVisitor :
    IExtractionVisitor<ModifiedIdentifierSyntax, SemanticModel, ILocalVariableType>
{
    public ILocalVariableType Visit(ModifiedIdentifierSyntax syntaxNode, SemanticModel semanticModel,
        ILocalVariableType modelType)
    {
        var variableDeclaratorSyntax = syntaxNode.GetParentDeclarationSyntax<VariableDeclaratorSyntax>();
        if (variableDeclaratorSyntax is null)
        {
            return modelType;
        }

        TypeSyntax variableTypeSyntax;
        switch (variableDeclaratorSyntax.AsClause)
        {
            case AsNewClauseSyntax asNewClauseSyntax:
                switch (asNewClauseSyntax.NewExpression)
                {
                    case ArrayCreationExpressionSyntax arrayCreationExpressionSyntax:
                        variableTypeSyntax = arrayCreationExpressionSyntax.Type;
                        break;
                    case ObjectCreationExpressionSyntax objectCreationExpressionSyntax:
                        variableTypeSyntax = objectCreationExpressionSyntax.Type;
                        break;
                    default:
                        return modelType;
                }

                break;
            case SimpleAsClauseSyntax simpleAsClauseSyntax:
                variableTypeSyntax = simpleAsClauseSyntax.Type;
                break;

            default:
                return modelType;
        }

        modelType.Name = syntaxNode.Identifier.ToString();
        var localVariableType = GetFullName(variableTypeSyntax, semanticModel, out var isNullable);

        modelType.Type = localVariableType;
        modelType.IsNullable = isNullable;

        return modelType;
    }
}
