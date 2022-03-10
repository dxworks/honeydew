namespace HoneydewScriptBeePlugin.Models;

public class ReturnValueModel : ReferenceEntity
{
    public EntityType Type { get; set; }

    public string Modifier { get; set; } = "";

    public bool IsNullable => Type is { IsNullable: true };

    public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();
}
