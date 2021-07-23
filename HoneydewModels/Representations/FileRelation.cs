namespace HoneydewModels.Representations
{
    public record FileRelation
    {
        public string FileTarget { get; set; } = "";

        public string RelationType { get; set; } = "";
        public int RelationCount { get; set; } = 0;
    }
}
