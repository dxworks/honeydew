namespace HoneydewScriptBeePlugin.Models;

public class InterfaceModel : EntityModel
{
    public bool IsPartial => Modifiers.Contains(HoneydewScriptBeePlugin.Models.Modifier.Partial);

    public IList<InterfaceModel> Partials { get; set; } = new List<InterfaceModel>();

    public IList<GenericParameterModel> GenericParameters { get; set; } = new List<GenericParameterModel>();

    public IList<EntityType> BaseTypes { get; set; } = new List<EntityType>();

    public new IList<PropertyModel> Properties { get; set; } = new List<PropertyModel>();

    public IList<MethodModel> Methods { get; set; } = new List<MethodModel>();
}
