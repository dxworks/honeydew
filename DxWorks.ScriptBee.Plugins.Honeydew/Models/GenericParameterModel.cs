namespace DxWorks.ScriptBee.Plugins.Honeydew.Models;

public class GenericParameterModel : ReferenceEntity
{
    public string Name { get; set; }

    public GenericParameterModifier Modifier { get; set; } = GenericParameterModifier.None;

    public IList<EntityType> Constraints { get; set; } = new List<EntityType>();

    public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();
}
