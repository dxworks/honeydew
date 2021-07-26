using HoneydewModels;

namespace HoneydewExtractors.Core.Metrics
{
    public interface IMetric
    {
        IMetricValue GetMetric();

        string PrettyPrint();
    }
}
