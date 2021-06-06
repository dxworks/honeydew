using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics
{
    public class UsingsCountMetric : CSharpMetricExtractor
    {
        private int _usingsCount = 0;

        public override string GetName()
        {
            return "Usings Count";
        }

        public override IMetric GetMetric()
        {
            return new Metric<int>(_usingsCount);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            _usingsCount++;
        }
    }
}