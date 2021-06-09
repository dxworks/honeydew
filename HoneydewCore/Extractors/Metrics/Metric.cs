namespace HoneydewCore.Extractors.Metrics
{
    public interface IMetric
    {
    }

    public record Metric<T> : IMetric
    {
        public T Value { get; }

        public Metric(T value)
        {
            Value = value;
        }
    }
}