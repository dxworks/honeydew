using System.Collections.Generic;

namespace HoneydewModels.Representations.ReferenceModel
{
    public record ReferenceMethodModel : ReferenceEntity
    {
        public ReferenceClassModel ContainingClass { get; init; }

        public bool IsConstructor { get; set; }
        
        public string Modifier { get; set; } = "";

        public string AccessModifier { get; set; }

        public ReferenceClassModel ReturnTypeReferenceClassModel { get; set; }

        public IList<ReferenceParameterModel> ParameterTypes { get; init; } = new List<ReferenceParameterModel>();

        public IList<ReferenceMethodModel> CalledMethods { get; } = new List<ReferenceMethodModel>();
    }
}
