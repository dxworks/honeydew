namespace Honeydew.Models.Types;

public interface IImportType : INamedType
{
    public string Alias { get; init; }

    public string AliasType { get; set; }

    public bool IsStatic { get; init; }
}

public enum EAliasType
{
    None,
    Namespace,
    Class,
    NotDetermined
}
