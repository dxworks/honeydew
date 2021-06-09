namespace HoneydewCore.Extractors.Metrics
{
    public interface IMetric
    {
        string GetValueType();

        object GetValue();
    }

    public record Metric<T> : IMetric
    {
        public T Value { get; }

        public Metric(T value)
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