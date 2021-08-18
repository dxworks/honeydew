using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record ReturnTypeModel : IModelEntity, IReturnType
    {
        public string Name { get; set; }

        public string Modifier { get; set; } = "";

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
    }
}
