using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpParameterRelationMetric : CSharpRelationMetric
    {
        public override string PrettyPrint()
        {
            return "Parameter Dependency";
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            ExtractDependencies(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            ExtractDependencies(node);
        }

        private void ExtractDependencies(BaseMethodDeclarationSyntax node)
        {
            foreach (var parameterSyntax in node.ParameterList.Parameters)
            {
                AddDependency(HoneydewSemanticModel.GetFullName(parameterSyntax.Type));
            }
        }
    }
}
