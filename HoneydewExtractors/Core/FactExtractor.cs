using System.Collections.Generic;
using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewModels;

namespace HoneydewExtractors.Core
{
    public abstract class
        FactExtractor<TClassModel, TSyntacticModel, TSemanticModel, TSyntaxNode> : IFactExtractor<TClassModel>
        where TClassModel : IClassModel
        where TSyntacticModel : ISyntacticModel
        where TSemanticModel : ISemanticModel
        where TSyntaxNode : ISyntaxNode
    {
        private readonly TypeSpawner<IExtractionMetric<TSyntacticModel, TSemanticModel, TSyntaxNode>> _metricSpawner =
            new();

        private readonly ISyntacticModelCreator<TSyntacticModel> _syntacticModelCreator;
        private readonly ISemanticModelCreator<TSyntacticModel, TSemanticModel> _semanticModelCreator;

        private readonly IClassModelExtractor<TClassModel, TSyntacticModel, TSemanticModel, TSyntaxNode>
            _classModelExtractor;

        protected FactExtractor(ISyntacticModelCreator<TSyntacticModel> syntacticModelCreator,
            ISemanticModelCreator<TSyntacticModel, TSemanticModel> semanticModelCreator,
            IClassModelExtractor<TClassModel, TSyntacticModel, TSemanticModel, TSyntaxNode>
                classModelExtractor)
        {
            _syntacticModelCreator = syntacticModelCreator;
            _semanticModelCreator = semanticModelCreator;
            _classModelExtractor = classModelExtractor;
        }

        public void AddMetric<T>() where T : IExtractionMetric<TSyntacticModel, TSemanticModel, TSyntaxNode>
        {
            _metricSpawner.LoadType<T>();
        }

        public IList<TClassModel> Extract(string fileContent)
        {
            var syntacticModel = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntacticModel);

            return _classModelExtractor.Extract(syntacticModel, semanticModel, _metricSpawner);
        }
    }
}
