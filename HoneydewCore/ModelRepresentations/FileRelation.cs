namespace HoneydewCore.ModelRepresentations
{
    public record FileRelation
    {
        public string Type { get; set; }

        public string FileSource { get; set; } = "";

        public string FileTarget { get; set; } = "";

        public int RelationCount { get; set; }
    }
}
