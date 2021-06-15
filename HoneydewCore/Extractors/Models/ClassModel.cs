namespace HoneydewCore.Extractors.Models
{
    public class ClassModel
    {
        public MetricsSet Metrics { get; } = new();

        public string Name { get; init; }
        public string Namespace { get; init; }

        public string FilePath { get; set; }
    }
}