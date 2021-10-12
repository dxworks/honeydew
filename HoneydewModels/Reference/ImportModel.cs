namespace HoneydewModels.Reference
{
    public class ImportModel : ReferenceEntity
    {
        public string Name { get; set; }

        public bool IsStatic { get; init; }

        public string Alias { get; init; } = "";

        public string AliasType { get; set; } = "";
    }
}
