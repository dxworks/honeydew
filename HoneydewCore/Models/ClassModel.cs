namespace HoneydewCore.Models
{
    public class ClassModel : ProjectEntity
    {
        public MetricsSet Metrics { get; } = new();

        public string Namespace { get; init; }
    }
}