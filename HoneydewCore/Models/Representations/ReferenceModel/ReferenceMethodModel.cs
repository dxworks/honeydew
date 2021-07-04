using System.Collections.Generic;

namespace HoneydewCore.Models.Representations.ReferenceModel
{
    public record ReferenceMethodModel : ReferenceEntity
    {
        public ReferenceClassModel ContainingClass { get; init; }

        public string Modifier { get; set; } = "";

        public string AccessModifier { get; set; }

        public ReferenceClassModel ReturnTypeReferenceClassModel { get; set; }

        public IList<ReferenceClassModel> ParameterTypes { get; init; } = new List<ReferenceClassModel>();

        public IList<ReferenceMethodModel> CalledMethods { get; } = new List<ReferenceMethodModel>();
    }
}