namespace HoneydewModels.Reference;

public class LocalVariableModel : ReferenceEntity
{
    public EntityType Type { get; set; }

    public bool IsNullable => Type is { IsNullable: true };

    public string Name { get; set; }
    
    public string Modifier { get; set; }
}
