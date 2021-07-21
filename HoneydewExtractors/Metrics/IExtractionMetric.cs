using HoneydewModels;

namespace HoneydewExtractors.Metrics
{
    public interface IExtractionMetric : IMetric
    {
        MetricValue Calculate(ISyntacticModel syntacticModel, ISemanticModel semanticModel);
    }
}
