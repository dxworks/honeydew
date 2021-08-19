using System.Collections.Generic;

namespace HoneydewModels.CSharp.ReferenceModel
{
    public record ReferenceClassModel : ReferenceEntity
    {
        public string ClassType { get; set; }
        
        public string FilePath { get; set; }
        
        public string AccessModifier { get; set; }

        public string Modifier { get; set; } = "";
        
        public ReferenceNamespaceModel NamespaceReference { get; init; }

        public ReferenceClassModel BaseClass { get; set; }

        public IList<ReferenceClassModel> BaseInterfaces { get; } = new List<ReferenceClassModel>();

        public IList<ReferenceFieldModel> Fields { get; } = new List<ReferenceFieldModel>();

        public IList<ReferenceMethodModel> Methods { get; } = new List<ReferenceMethodModel>();
        
        public IList<ReferenceMethodModel> Constructors { get; } = new List<ReferenceMethodModel>();

        public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
    }
}
