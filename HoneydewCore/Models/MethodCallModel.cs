namespace HoneydewCore.Models
{
    public record MethodCallModel
    {
        public string MethodName { get; init; }
        public string ContainingClassName { get; set; }
    }
}