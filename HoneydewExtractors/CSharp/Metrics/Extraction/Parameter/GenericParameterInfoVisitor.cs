using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Parameter
{
    public class GenericParameterInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpGenericParameterVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IGenericParameterType Visit(TypeParameterSyntax syntaxNode, IGenericParameterType modelType)
        {
            modelType.Name = syntaxNode.Identifier.ToString();
            modelType.Modifier = syntaxNode.VarianceKeyword.ToString();

            var constraintsSyntaxes = new List<TypeParameterConstraintSyntax>();

            var delegateParent = syntaxNode.GetParentDeclarationSyntax<DelegateDeclarationSyntax>();
            if (delegateParent != null)
            {
                foreach (var constraintClause in delegateParent.ConstraintClauses)
                {
                    if (constraintClause.Name.Identifier.ToString() != syntaxNode.Identifier.ToString())
                    {
                        continue;
                    }

                    foreach (var constraint in constraintClause.Constraints)
                    {
                        constraintsSyntaxes.Add(constraint);
                    }
                }
            }

            var baseTypeParent = syntaxNode.GetParentDeclarationSyntax<BaseTypeDeclarationSyntax>();
            if (baseTypeParent != null)
            {
                foreach (var constraintClause in baseTypeParent.ChildNodes()
                    .OfType<TypeParameterConstraintClauseSyntax>())
                {
                    if (constraintClause.Name.Identifier.ToString() != syntaxNode.Identifier.ToString())
                    {
                        continue;
                    }

                    foreach (var constraint in constraintClause.Constraints)
                    {
                        constraintsSyntaxes.Add(constraint);
                    }
                }
            }

            foreach (var constrainsSyntax in constraintsSyntaxes)
            {
                modelType.Constraints.Add(CSharpHelperMethods.GetFullName(constrainsSyntax));
            }

            return modelType;
        }
    }
}
