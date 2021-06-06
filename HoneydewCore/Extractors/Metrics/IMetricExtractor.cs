namespace HoneydewCore.Extractors.Metrics
{
    public interface IMetricExtractor
    {
        string GetName();

        IMetric GetMetric();
    }
}