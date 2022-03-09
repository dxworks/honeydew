using System.Collections.Generic;

namespace HoneydewModels.Reference;

public class EntityType : ReferenceEntity
{
    public EntityModel Entity { get; set; }

    public string Name { get; set; }

    public IList<EntityType> GenericTypes { get; set; } = new List<EntityType>();

    public bool IsNullable { get; set; }

    public bool IsGeneric => GenericTypes.Count > 0;

    public bool IsExtern => Entity is { IsExternal: true };

    public bool IsInternal => Entity is { IsInternal: true };

    public bool IsPrimitive => Entity is { IsPrimitive: true };
}
