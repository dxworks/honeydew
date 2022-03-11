using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp;

public record EnumLabelType : IEnumLabelType
{
    public string Name { get; set; }

    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
}
