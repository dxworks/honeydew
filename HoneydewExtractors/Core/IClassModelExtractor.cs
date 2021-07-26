using System.Collections.Generic;
using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewModels;

namespace HoneydewExtractors.Core
{
    public interface IClassModelExtractor<TClassModel, TSyntacticModel, TSemanticModel, TSyntaxNode>
        where TClassModel : IClassModel
        where TSyntacticModel : ISyntacticModel
        where TSemanticModel : ISemanticModel
        where TSyntaxNode : ISyntaxNode
    {
        IList<TClassModel> Extract(TSyntacticModel syntacticModel, TSemanticModel semanticModel,
            TypeSpawner<IExtractionMetric<TSyntacticModel, TSemanticModel, TSyntaxNode>> metricSpawner);
    }
}
