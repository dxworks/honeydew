namespace HoneydewCore.Extractors.Metrics
{
    public interface IMetricExtractor
    {
        MetricType GetMetricType();

        string GetName();

        IMetric GetMetric();
    }
}