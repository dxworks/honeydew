using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Utils;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class GenericParameterInfoVisitor : IExtractionVisitor<TypeParameterSyntax, SemanticModel, IGenericParameterType>
{
    public IGenericParameterType Visit(TypeParameterSyntax syntaxNode, SemanticModel semanticModel,
        IGenericParameterType modelType)
    {
        modelType.Name = syntaxNode.Identifier.ToString();
        modelType.Modifier = syntaxNode.VarianceKeyword.ToString();

        ExtractConstraints(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    private void ExtractConstraints(TypeParameterSyntax syntaxNode, SemanticModel semanticModel,
        IGenericParameterType modelType)
    {
        var constraintsSyntaxNodes = syntaxNode.ChildNodes()
            .OfType<TypeParameterConstraintClauseSyntax>();

        foreach (var constrainsSyntax in constraintsSyntaxNodes)
        {
            switch (constrainsSyntax)
            {
                case TypeParameterMultipleConstraintClauseSyntax typeParameterMultipleConstraintClauseSyntax:
                    foreach (var constraintSyntax in typeParameterMultipleConstraintClauseSyntax.Constraints)
                    {
                        modelType.Constraints.Add(
                            VisualBasicExtractionHelperMethods.GetFullName(constraintSyntax, semanticModel));
                    }

                    break;
                case TypeParameterSingleConstraintClauseSyntax typeParameterSingleConstraintClauseSyntax:
                    modelType.Constraints.Add(
                        VisualBasicExtractionHelperMethods.GetFullName(
                            typeParameterSingleConstraintClauseSyntax.Constraint, semanticModel));
                    break;
            }
        }
    }
}
