using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp;

public record GenericParameterModel : IGenericParameterType
{
    public string Name { get; set; }

    public string Modifier { get; set; }

    public IList<IEntityType> Constraints { get; set; } = new List<IEntityType>();

    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
}
