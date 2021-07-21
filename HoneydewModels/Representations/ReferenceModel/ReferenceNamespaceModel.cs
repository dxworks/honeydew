using System.Collections.Generic;

namespace HoneydewModels.Representations.ReferenceModel
{
    public record ReferenceNamespaceModel  : ReferenceEntity
    {
        public ReferenceProjectModel ProjectReference { get; init; }
        public IList<ReferenceClassModel> ClassModels { get; } = new List<ReferenceClassModel>();
    }
}
