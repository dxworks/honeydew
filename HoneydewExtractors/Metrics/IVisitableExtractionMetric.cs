using HoneydewExtractors.Metrics.Extraction;

namespace HoneydewExtractors.Metrics
{
    public interface IVisitableExtractionMetric<in TSyntaxNode> : IMetric
        where TSyntaxNode : ISyntaxNode
    {
        void Visit(TSyntaxNode syntaxNode);
    }
}
