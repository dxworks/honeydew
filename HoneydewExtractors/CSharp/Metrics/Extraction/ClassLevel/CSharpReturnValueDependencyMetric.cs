using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpReturnValueDependencyMetric : CSharpDependencyMetric
    {
        public override string PrettyPrint()
        {
            return "Return Value Dependency";
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            AddDependency(HoneydewSemanticModel.GetFullName(node.ReturnType));
        }
    }
}
