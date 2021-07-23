using HoneydewModels;

namespace HoneydewExtractors.Metrics
{
    public interface IMetric
    {
        IMetricValue GetMetric();

        string PrettyPrint();
    }
}
