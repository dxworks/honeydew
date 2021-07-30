namespace HoneydewExtractors.Core.Metrics.Extraction
{
    public interface IVisitableExtractionMetric<in TSyntaxNode> : IMetric
        where TSyntaxNode : ISyntaxNode
    {
        void Visit(TSyntaxNode syntaxNode);
    }
}
