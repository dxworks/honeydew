using Microsoft.CodeAnalysis;

namespace HoneydewModels
{
    public interface IClassModel
    {
        void AddMetricValue(string extractorName, IMetricValue metricValue);

        Optional<object> GetMetricValue<T>();
    }
}
