namespace HoneydewModels
{
    public record ClassMetric
    {
        public string ExtractorName { get; set; }
        public string ValueType { get; set; }
        public object Value { get; set; }
    }
}
