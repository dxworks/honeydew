using System.Collections.Generic;

namespace HoneydewModels.Reference;

public class FieldModel : ReferenceEntity
{
    public string Name { get; set; }

    public EntityModel Entity { get; set; }

    public AccessModifier AccessModifier { get; set; }

    public string Modifier { get; set; } = "";

    public IList<Modifier> Modifiers { get; set; } = new List<Modifier>();

    public EntityType Type { get; set; }

    public bool IsEvent { get; set; }

    public IList<FieldAccess> Accesses { get; set; } = new List<FieldAccess>();

    public bool IsNullable => Type is { IsNullable: true };

    public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();

    public IDictionary<string, int> Metrics { get; init; } = new Dictionary<string, int>();
}
