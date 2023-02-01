namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public record UsingModel_V210
{
    public string Name { get; set; } = null!;

    public bool IsStatic { get; init; }

    public string Alias { get; init; } = "";

    public string AliasType { get; set; } = nameof(EAliasType.None);
}

public enum EAliasType
{
    None,
    Namespace,
    Class,
    NotDetermined
}
