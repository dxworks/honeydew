using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp
{
    public class CSharpParameterDependencyMetric : CSharpDependencyMetric, IParameterDependencyMetric
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
