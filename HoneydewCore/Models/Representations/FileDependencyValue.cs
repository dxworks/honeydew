namespace HoneydewCore.Models.Representations
{
    public record FileDependencyValue
    {
        public string RelationType { get; set; } = "";
        public int RelationCount { get; set; } = 0;
    }
}