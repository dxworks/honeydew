using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record FieldModel : IModelEntity, IFieldType
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Modifier { get; set; } = "";

        public string AccessModifier { get; set; }

        public string ContainingTypeName { get; set; }

        public bool IsEvent { get; set; }

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
    }
}
