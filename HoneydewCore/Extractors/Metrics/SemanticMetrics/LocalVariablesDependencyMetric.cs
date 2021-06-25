using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    public class LocalVariablesDependencyMetric : DependencyMetric
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
                var dependencySymbolInfo = SemanticModel.GetSymbolInfo(variableDeclarationSyntax.Type);
                if (dependencySymbolInfo.Symbol == null)
                {
                    if (variableDeclarationSyntax.Type.ToString() == "var")
                    {
                        foreach (var declarationVariable in variableDeclarationSyntax.Variables)
                        {
                            if (declarationVariable.Initializer is
                            {
                                Value: ObjectCreationExpressionSyntax
                                objectCreationExpressionSyntax
                            })
                            {
                                AddDependency(objectCreationExpressionSyntax.Type.ToString());
                            }
                        }
                    }
                    else
                    {
                        AddDependency(variableDeclarationSyntax.Type.ToString());
                    }
                }
                else
                {
                    AddDependency(dependencySymbolInfo.Symbol.ToString());
                }
            }
        }
    }
}