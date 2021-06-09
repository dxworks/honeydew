namespace HoneydewCore.Extractors.Metrics
{
    public interface IMetricExtractor
    {
        MetricType GetMetricType();

        IMetric GetMetric();
    }
}