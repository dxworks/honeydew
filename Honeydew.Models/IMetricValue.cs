namespace Honeydew.Models
{
    public interface IMetricValue
    {
        string GetValueType();

        object GetValue();
    }

    public record MetricValue<T> : IMetricValue
    {
        public T Value { get; }

        public MetricValue(T value)
        {
            Value = value;
        }

        public string GetValueType()
        {
            return typeof(T).ToString();
        }

        public object GetValue()
        {
            return Value;
        }
    }
}
