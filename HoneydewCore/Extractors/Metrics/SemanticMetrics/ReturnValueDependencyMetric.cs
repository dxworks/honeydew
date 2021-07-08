using Microsoft.CodeAnalysis;
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
            var dependencySymbolInfo = ExtractorSemanticModel.GetSymbolInfo(node.ReturnType);

            if (dependencySymbolInfo.Symbol == null)
            {
                var parameterType = node.ReturnType.ToString();
                AddDependency(parameterType);
            }
            else
            {
                AddDependency(dependencySymbolInfo.Symbol.ToString());
            }
        }
    }
}