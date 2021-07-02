using System.Collections.Generic;

namespace HoneydewCore.Models.Representations.ReferenceModel
{
    public record ReferenceMethodModel : ReferenceEntity
    {
        public ReferenceClassModel ContainingClass { get; init; }

        public string Modifier { get; init; } = "";

        public string AccessModifier { get; init; }

        public ReferenceClassModel ReturnTypeReferenceClassModel { get; init; }

        public IList<ReferenceClassModel> ParameterTypes { get; init; } = new List<ReferenceClassModel>();

        public IList<ReferenceMethodModel> CalledMethods { get; } = new List<ReferenceMethodModel>();
    }
}