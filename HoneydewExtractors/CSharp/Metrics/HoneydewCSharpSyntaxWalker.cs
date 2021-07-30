using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewModels;
using Microsoft.CodeAnalysis.CSharp;

namespace HoneydewExtractors.CSharp.Metrics
{
    public abstract class HoneydewCSharpSyntaxWalker : CSharpSyntaxWalker, IVisitableExtractionMetric<CSharpSyntaxNode>
    {
        public abstract IMetricValue GetMetric();
        public abstract string PrettyPrint();

        public void Visit(CSharpSyntaxNode syntaxNode)
        {
            Visit(syntaxNode.Node);
        }
    }
}
