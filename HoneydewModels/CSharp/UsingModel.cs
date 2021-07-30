namespace HoneydewModels.CSharp
{
    public record UsingModel
    {
        public string Name { get; set; }
        public bool IsStatic { get; init; }

        public string Alias { get; init; } = "";

        public EAliasType AliasType { get; set; }
    }

    public enum EAliasType
    {
        None,
        Namespace,
        Class,
        NotDetermined
    }
}
