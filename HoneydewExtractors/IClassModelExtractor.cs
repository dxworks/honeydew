using System.Collections.Generic;
using HoneydewExtractors.Metrics;
using HoneydewExtractors.Metrics.Extraction;
using HoneydewModels;

namespace HoneydewExtractors
{
    public interface IClassModelExtractor<TClassModel, TSyntacticModel, TSemanticModel, TSyntaxNode>
        where TClassModel : IClassModel
        where TSyntacticModel : ISyntacticModel
        where TSemanticModel : ISemanticModel
        where TSyntaxNode : ISyntaxNode
    {
        IList<TClassModel> Extract(TSyntacticModel syntacticModel, TSemanticModel semanticModel,
            MetricLoader<IExtractionMetric<TSyntacticModel, TSemanticModel, TSyntaxNode>> metricLoader);
    }
}
