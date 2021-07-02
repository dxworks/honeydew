using System.Collections.Generic;

namespace HoneydewCore.Models.Representations.ReferenceModel
{
    public record ReferenceClassModel : ReferenceEntity
    {
        public ReferenceNamespaceModel NamespaceReference { get; init; }

        public string FilePath { get; init; }
        public IList<ReferenceFieldModel> Fields { get; } = new List<ReferenceFieldModel>();
        public IList<ReferenceMethodModel> Methods { get; } = new List<ReferenceMethodModel>();
        public IList<ClassMetric> Metrics { get; init; } = new List<ClassMetric>();
    }
}