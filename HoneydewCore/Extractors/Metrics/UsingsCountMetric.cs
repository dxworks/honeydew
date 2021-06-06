using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics
{
    public class UsingsCountMetric : CSharpMetricExtractor
    {
        private int usingsCount = 0;

        public override string GetName()
        {
            return "Usings Count";
        }

        public override int GetMetric()
        {
            return usingsCount;
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            usingsCount++;
        }
    }
}