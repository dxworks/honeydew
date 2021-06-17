namespace HoneydewCore.Models.Representations
{
    public record FileRelation
    {
        public string FileSource { get; set; } = "";
        public string FileTarget { get; set; } = "";

        public string RelationType { get; set; } = "";
        public int RelationCount { get; set; } = 0;
    }
}