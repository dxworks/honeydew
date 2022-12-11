namespace DxWorks.ScriptBee.Plugins.Honeydew.Models;

public class AttributeModel : ReferenceEntity
{
    public EntityType Type { get; set; }

    public AttributeTarget Target { get; set; } = AttributeTarget.None;

    public IList<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();
}
