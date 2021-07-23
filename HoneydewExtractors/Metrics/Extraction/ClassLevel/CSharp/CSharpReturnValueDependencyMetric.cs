using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp
{
    public class CSharpReturnValueDependencyMetric : CSharpDependencyMetric, IReturnValueDependencyMetric
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
