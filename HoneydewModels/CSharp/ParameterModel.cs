using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record ParameterModel : IModelEntity, IParameterType
    {
        public IEntityType Type { get; set; }

        public string Modifier { get; init; } = "";

        public string DefaultValue { get; init; }

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
    }
}
