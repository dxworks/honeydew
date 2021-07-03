using System.Collections.Generic;

namespace HoneydewCore.Models.Representations.ReferenceModel
{
    public record ReferenceClassModel : ReferenceEntity
    {
        public string ClassType { get; init; }
        
        public string FilePath { get; init; }
        
        public string AccessModifier { get; init; }

        public string Modifier { get; init; } = "";
        
        public ReferenceNamespaceModel NamespaceReference { get; init; }

        public ReferenceClassModel BaseClass { get; set; }

        public IList<ReferenceClassModel> BaseInterfaces { get; } = new List<ReferenceClassModel>();

        public IList<ReferenceFieldModel> Fields { get; } = new List<ReferenceFieldModel>();

        public IList<ReferenceMethodModel> Methods { get; } = new List<ReferenceMethodModel>();

        public IList<ClassMetric> Metrics { get; init; } = new List<ClassMetric>();
    }
}