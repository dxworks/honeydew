using System.Collections.Generic;
using HoneydewExtractors.Metrics;
using HoneydewModels;

namespace HoneydewExtractors
{
    public class FactExtractor : IFactExtractor
    {
        private readonly IMetricLoader<IExtractionMetric> _metricLoader;
        private readonly ISyntacticModelCreator _syntacticModelCreator;
        private readonly ISemanticModelCreator _semanticModelCreator;
        private readonly IClassModelExtractor _classModelExtractor;

        public FactExtractor(IMetricLoader<IExtractionMetric> metricLoader,
            ISyntacticModelCreator syntacticModelCreator, ISemanticModelCreator semanticModelCreator,
            IClassModelExtractor classModelExtractor)
        {
            _metricLoader = metricLoader;
            _syntacticModelCreator = syntacticModelCreator;
            _semanticModelCreator = semanticModelCreator;
            _classModelExtractor = classModelExtractor;
        }

        public IList<IClassModel> Extract(string fileContent)
        {
            var syntacticModel = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntacticModel);

            var classModels = _classModelExtractor.Extract(syntacticModel, semanticModel);

            var extractionMetrics = _metricLoader.GetMetrics();
            foreach (var classModel in classModels)
            {
                foreach (var extractionMetric in extractionMetrics)
                {
                    classModel.AddMetricValue(extractionMetric.Calculate(syntacticModel, semanticModel));
                }
            }

            return classModels;
        }
    }
}
