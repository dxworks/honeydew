using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public class AttributeModel : IAttributeType
    {
        public string Name { get; set; }
        
        public IEntityType Type { get; set; }

        public string ContainingTypeName { get; set; }

        public IList<IParameterType> ParameterTypes { get; set; } = new List<IParameterType>();

        public string Target { get; set; }
    }
}
