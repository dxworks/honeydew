namespace HoneydewModels.Reference;

public class ImportModel : ReferenceEntity
{
    public NamespaceModel? Namespace { get; set; }

    public EntityModel? Entity { get; set; }

    public bool IsStatic { get; init; }

    public string Alias { get; init; } = "";

    public AliasType AliasType { get; set; } = AliasType.None;
}
