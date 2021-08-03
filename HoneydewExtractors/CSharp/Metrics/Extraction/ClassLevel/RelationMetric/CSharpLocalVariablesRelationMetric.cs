using System.Linq;
using HoneydewExtractors.CSharp.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpLocalVariablesRelationMetric : CSharpRelationMetric
    {
        public override string PrettyPrint()
        {
            return "Local Variables Dependency";
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            if (node.Body == null) return;

            ExtractDependencies(node.Body);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.Body == null) return;

            ExtractDependencies(node.Body);
        }

        private void ExtractDependencies(SyntaxNode body)
        {
            foreach (var variableDeclarationSyntax in body.DescendantNodes()
                .OfType<VariableDeclarationSyntax>())
            {
                var fullName = HoneydewSemanticModel.GetFullName(variableDeclarationSyntax.Type);

                if (fullName != CSharpConstants.VarIdentifier)
                {
                    AddDependency(fullName);
                }
                else
                {
                    fullName = HoneydewSemanticModel.GetFullName(variableDeclarationSyntax);
                    if (fullName != CSharpConstants.VarIdentifier)
                    {
                        AddDependency(fullName);
                    }
                    else
                    {
                        foreach (var declarationVariable in variableDeclarationSyntax.Variables)
                        {
                            if (declarationVariable.Initializer is
                            {
                                Value: ObjectCreationExpressionSyntax
                                objectCreationExpressionSyntax
                            })
                            {
                                AddDependency(HoneydewSemanticModel.GetFullName(objectCreationExpressionSyntax.Type));
                            }
                        }
                    }
                }
            }
        }
    }
}
