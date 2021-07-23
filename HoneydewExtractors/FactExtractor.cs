using System;
using System.Collections.Generic;
using HoneydewExtractors.Metrics;
using HoneydewExtractors.Metrics.Extraction;
using HoneydewModels;

namespace HoneydewExtractors
{
    public abstract class
        FactExtractor<TClassModel, TSyntacticModel, TSemanticModel, TSyntaxNode> : IFactExtractor<TClassModel>
        where TClassModel : IClassModel
        where TSyntacticModel : ISyntacticModel
        where TSemanticModel : ISemanticModel
        where TSyntaxNode : ISyntaxNode
    {
        private readonly ISet<Type> _abstractMetrics = new HashSet<Type>();

        private readonly MetricLoader<IExtractionMetric<TSyntacticModel, TSemanticModel, TSyntaxNode>> _metricLoader =
            new();

        private readonly ISyntacticModelCreator<TSyntacticModel> _syntacticModelCreator;
        private readonly ISemanticModelCreator<TSyntacticModel, TSemanticModel> _semanticModelCreator;

        private readonly IClassModelExtractor<TClassModel, TSyntacticModel, TSemanticModel, TSyntaxNode>
            _classModelExtractor;
        
        private bool _hasLoadedMetrics;

        protected abstract IDictionary<Type, Type> PopulateWithConcreteTypes();

        protected FactExtractor(ISyntacticModelCreator<TSyntacticModel> syntacticModelCreator,
            ISemanticModelCreator<TSyntacticModel, TSemanticModel> semanticModelCreator,
            IClassModelExtractor<TClassModel, TSyntacticModel, TSemanticModel, TSyntaxNode>
                classModelExtractor)
        {
            _syntacticModelCreator = syntacticModelCreator;
            _semanticModelCreator = semanticModelCreator;
            _classModelExtractor = classModelExtractor;
            }

        public void AddMetric<T>() where T : IMetric
        {
            _abstractMetrics.Add(typeof(T));
        }

        public IList<TClassModel> Extract(string fileContent)
        {
            if (!_hasLoadedMetrics)
            {
                _hasLoadedMetrics = true;
                
                var concreteMetrics = PopulateWithConcreteTypes();
                foreach (var abstractMetric in _abstractMetrics)
                {
                    if (concreteMetrics.TryGetValue(abstractMetric, out var concreteMetricType))
                    {
                        _metricLoader.LoadMetric(concreteMetricType);
                    }
                }
            }

            var syntacticModel = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntacticModel);

            return _classModelExtractor.Extract(syntacticModel, semanticModel, _metricLoader);
        }
    }
}
