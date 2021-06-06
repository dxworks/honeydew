namespace HoneydewCore.Extractors.Metrics
{
    public interface IMetricExtractor
    {
        bool IsSemantic();

        string GetName();

        IMetric GetMetric();
    }
}