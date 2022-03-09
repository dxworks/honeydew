using HoneydewModels.Types;

namespace HoneydewModels.CSharp;

public class EntityTypeModel : IEntityType
{
    public string Name { get; set; }

    public GenericType FullType { get; set; }
}
