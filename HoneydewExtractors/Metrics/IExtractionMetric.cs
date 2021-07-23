using HoneydewExtractors.Metrics.Extraction;

namespace HoneydewExtractors.Metrics
{
    public interface
        IExtractionMetric<TSyntacticModel, TSemanticModel, in TSyntaxNode> : IVisitableExtractionMetric<TSyntaxNode>
        where TSyntacticModel : ISyntacticModel
        where TSemanticModel : ISemanticModel
        where TSyntaxNode : ISyntaxNode
    {
        public ExtractionMetricType GetMetricType();

        public TSyntacticModel HoneydewSyntacticModel { get; set; }
        public TSemanticModel HoneydewSemanticModel { get; set; }
    }
}
