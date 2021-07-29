namespace HoneydewModels.CSharp
{
    public record UsingModel
    {
        public string Name { get; set; }
        public bool IsStatic { get; set; }

        public string Alias { get; set; } = "";

        public EAliasType AliasType { get; set; }
    }

    public enum EAliasType
    {
        None,
        Namespace,
        Class
    }
}
