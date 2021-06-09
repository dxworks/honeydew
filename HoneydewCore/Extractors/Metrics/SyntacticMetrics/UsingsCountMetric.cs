using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SyntacticMetrics
{
    public class UsingsCountMetric : CSharpMetricExtractor
    {
        private int _usingsCount;

        public override IMetric GetMetric()
        {
            return new Metric<int>(_usingsCount);
        }

        public override MetricType GetMetricType()
        {
            return MetricType.Syntactic;
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            _usingsCount++;
        }
    }
}