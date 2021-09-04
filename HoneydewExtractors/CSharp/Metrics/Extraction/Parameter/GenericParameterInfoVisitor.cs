using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
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

            ExtractConstraints(syntaxNode, modelType);

            return modelType;
        }

        private void ExtractConstraints(TypeParameterSyntax syntaxNode, IGenericParameterType modelType)
        {
            var constraintsSyntaxes = new List<TypeParameterConstraintSyntax>();

            ExtractConstraintsFromParent<DelegateDeclarationSyntax>();
            ExtractConstraintsFromParent<BaseTypeDeclarationSyntax>();
            ExtractConstraintsFromParent<BaseMethodDeclarationSyntax>();

            foreach (var constrainsSyntax in constraintsSyntaxes)
            {
                modelType.Constraints.Add(CSharpHelperMethods.GetFullName(constrainsSyntax));
            }

            void ExtractConstraintsFromParent<T>() where T : SyntaxNode
            {
                var baseTypeParent = syntaxNode.GetParentDeclarationSyntax<T>();
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
            }
        }
    }
}
