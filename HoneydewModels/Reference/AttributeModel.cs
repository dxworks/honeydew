using System.Collections.Generic;

namespace HoneydewModels.Reference;

public class AttributeModel : ReferenceEntity
{
    public EntityType Type { get; set; }

    public AttributeTarget Target { get; set; } = AttributeTarget.None;

    public IList<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();

    public IList<EntityType> GenericParameters { get; set; } = new List<EntityType>();
}
