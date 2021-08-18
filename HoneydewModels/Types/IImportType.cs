namespace HoneydewModels.Types
{
    public interface IImportType : IType
    {
        public string Alias { get; init; }

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
