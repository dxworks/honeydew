using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record FieldModel : IFieldType
    {
        public string Name { get; set; }

        public IEntityType Type { get; set; }

        public string Modifier { get; set; } = "";

        public string AccessModifier { get; set; }

        public string ContainingTypeName { get; set; }

        public bool IsEvent { get; set; }

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();

        public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
    }
}
