namespace Honeydew.ScriptBeePlugin.Models;

public class ClassModel : EntityModel
{
    public ClassType Type { get; set; }

    public bool IsPartial => Modifiers.Contains(Models.Modifier.Partial);

    public IList<ClassModel> Partials { get; set; } = new List<ClassModel>();

    public IList<GenericParameterModel> GenericParameters { get; set; } = new List<GenericParameterModel>();

    public IList<EntityType> BaseTypes { get; set; } = new List<EntityType>();

    public IList<FieldModel> Fields { get; set; } = new List<FieldModel>();

    public new IList<PropertyModel> Properties { get; set; } = new List<PropertyModel>();

    public IList<MethodModel> Methods { get; set; } = new List<MethodModel>();

    public IList<MethodModel> Constructors { get; set; } = new List<MethodModel>();

    public MethodModel? Destructor { get; set; }
}
