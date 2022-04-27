﻿using Honeydew.Models.Types;

namespace Honeydew.Models.VisualBasic;

public record VisualBasicParameterModel : IParameterType
{
    public IEntityType Type { get; set; }

    public string Modifier { get; set; } = "";

    public string? DefaultValue { get; set; }

    public bool IsNullable { get; set; }

    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
}
