using System.Collections.Generic;

namespace HoneydewModels.CSharp.ReferenceModel
{
    public record ReferenceCompilationUnitModel  : ReferenceEntity
    {
        public string FileName { get; set; }
        
        public ReferenceProjectModel ProjectReference { get; init; }
        
        public IList<ReferenceClassModel> ClassModels { get; } = new List<ReferenceClassModel>();
    }
}
