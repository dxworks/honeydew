using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class FieldModel : ReferenceEntity
    {
        public string Name { get; set; }

        public EntityType Type { get; set; }

        public string Modifier { get; set; } = "";

        public string AccessModifier { get; set; }

        public ClassModel Class { get; set; }

        public bool IsEvent { get; set; }

        public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();

        public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
    }
}
