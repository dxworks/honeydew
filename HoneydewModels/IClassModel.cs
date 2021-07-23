namespace HoneydewModels
{
    public interface IClassModel
    {
        void AddMetricValue(string extractorName, IMetricValue metricValue);
    }
}
