using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record FieldModel : IModelEntity, IFieldType
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Modifier { get; init; } = "";

        public string AccessModifier { get; init; }

        public string ContainingTypeName { get; set; }

        public bool IsEvent { get; init; }

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
    }
}
