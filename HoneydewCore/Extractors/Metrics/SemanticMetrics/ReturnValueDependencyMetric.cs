using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    public class ReturnValueDependencyMetric : DependencyMetric
    {
        public override string PrettyPrint()
        {
            return "Return Value Dependency";
        }
        
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var parameterType = node.ReturnType.ToString();
            AddDependency(parameterType);
        }
    }
}