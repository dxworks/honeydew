using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record ParameterModel : IModelEntity, IParameterType
    {
        public IEntityType Type { get; set; }

        public string Modifier { get; set; } = "";

        public string DefaultValue { get; set; }

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
    }
}
