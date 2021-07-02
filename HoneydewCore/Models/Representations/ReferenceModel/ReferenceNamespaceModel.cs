using System.Collections.Generic;

namespace HoneydewCore.Models.Representations.ReferenceModel
{
    public record ReferenceNamespaceModel  : ReferenceEntity
    {
        public ReferenceProjectModel ProjectReference { get; init; }
        public IList<ReferenceClassModel> ClassModels { get; } = new List<ReferenceClassModel>();
    }
}