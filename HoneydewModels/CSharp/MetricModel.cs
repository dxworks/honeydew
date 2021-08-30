namespace HoneydewModels.CSharp
{
    public record MetricModel
    {
        public string ExtractorName { get; set; }
        public string ValueType { get; set; }
        public object Value { get; set; }
    }
}
