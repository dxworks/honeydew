using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    public class ReturnValueDependencyMetric : DependencyMetric, ISemanticMetric, ISyntacticMetric
    {
        public override IMetric GetMetric()
        {
            return new Metric<DependencyDataMetric>(DataMetric);
        }

        public override string PrettyPrint()
        {
            return "Return Value Dependency";
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            DataMetric.Usings.Add(node.Name.ToString());
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var parameterType = node.ReturnType.ToString();
            if (DataMetric.Dependencies.ContainsKey(parameterType))
            {
                DataMetric.Dependencies[parameterType]++;
            }
            else
            {
                DataMetric.Dependencies.Add(parameterType, 1);
            }
        }
    }
}