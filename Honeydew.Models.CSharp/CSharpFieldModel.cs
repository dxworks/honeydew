﻿using Honeydew.Models.Types;

namespace Honeydew.Models.CSharp;

public record CSharpFieldModel : IFieldType, IEventType
{
    public string Name { get; set; } = "";

    public IEntityType Type { get; set; }

    public string Modifier { get; set; } = "";

    public string AccessModifier { get; set; } = "";

    public bool IsEvent { get; set; }

    public bool IsNullable { get; set; }

    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();

    public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
}
