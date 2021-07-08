using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    public class ParameterDependencyMetric : DependencyMetric
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
                if (parameterSyntax.Type == null) continue;
                var dependencySymbolInfo = ExtractorSemanticModel.GetSymbolInfo(parameterSyntax.Type);

                if (dependencySymbolInfo.Symbol == null)
                {
                    var parameterType = parameterSyntax.Type.ToString();
                    AddDependency(parameterType);
                }
                else
                {
                    AddDependency(dependencySymbolInfo.Symbol.ToString());
                }
            }
        }
    }
}