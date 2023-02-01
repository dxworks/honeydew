﻿namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public class AttributeModel_V210
{
    public string Name { get; set; } = null!;

    public EntityTypeModel_V210 Type { get; set; } = null!;

    public string ContainingTypeName { get; set; } = null!;

    public IList<ParameterModel_V210> ParameterTypes { get; set; } = new List<ParameterModel_V210>();

    public string Target { get; set; } = null!;
}
