using System.Collections.Generic;

namespace HoneydewModels.Reference;

public class MethodCall : ReferenceEntity
{
    public MethodModel Called { get; set; }

    public MethodModel Caller { get; set; }

    public IList<EntityType> GenericParameters { get; set; } = new List<EntityType>();

    public IList<EntityType> ConcreteParameters { get; set; } = new List<EntityType>();
}
