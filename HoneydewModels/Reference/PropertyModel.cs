using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class PropertyModel : ReferenceEntity
    {
        public string Name { get; set; }

        public ClassModel Class { get; set; }

        public string AccessModifier { get; set; }

        public string Modifier { get; set; } = "";

        public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();

        public EntityType Type { get; set; }

        public bool IsEvent { get; set; }

        public bool IsNullable { get; set; }

        public IList<MethodModel> Accessors { get; set; } = new List<MethodModel>();

        public LinesOfCode Loc { get; set; }

        public int CyclomaticComplexity { get; set; }

        public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
    }
}
