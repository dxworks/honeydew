using System.Collections.Generic;

namespace HoneydewModels.Reference;

public class ParameterModel : ReferenceEntity
{
    public EntityType Type { get; set; }

    public string TypeName { get; set; }

    public ParameterModifier Modifier { get; set; } = ParameterModifier.None;

    public string? DefaultValue { get; set; }

    public bool IsNullable => Type is { IsNullable: true };

    public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();
}
