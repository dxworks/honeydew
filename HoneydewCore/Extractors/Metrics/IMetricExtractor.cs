namespace HoneydewCore.Extractors.Metrics
{
    public interface IMetricExtractor
    {
        IMetric GetMetric();

        string PrettyPrint();
    }
}