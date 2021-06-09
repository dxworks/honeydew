namespace HoneydewCore.Models
{
    public class ClassModel
    {
        public MetricsSet Metrics { get; } = new();

        public string Name { get; init; }
        public string Namespace { get; init; }
    }
}