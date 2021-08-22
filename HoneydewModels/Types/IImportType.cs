namespace HoneydewModels.Types
{
    public interface IImportType : INamedType
    {
        public string Alias { get; init; }

        public string AliasType { get; set; }
    }

    public enum EAliasType
    {
        None,
        Namespace,
        Class,
        NotDetermined
    }
}
