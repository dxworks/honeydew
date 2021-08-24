using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record PropertyModel : IModelEntity, IPropertyType, ITypeWithLocalFunctions
    {
        public int CyclomaticComplexity { get; set; }
        public string Name { get; set; }

        public string ContainingTypeName { get; set; }

        public string AccessModifier { get; set; }

        public string Modifier { get; set; } = "";

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();

        public IEntityType Type { get; set; }

        public bool IsEvent { get; set; }

        public IList<IMethodSignatureType> CalledMethods { get; set; } = new List<IMethodSignatureType>();

        public IList<string> Accessors { get; set; } = new List<string>();

        public IList<IMethodTypeWithLocalFunctions> LocalFunctions { get; set; } =
            new List<IMethodTypeWithLocalFunctions>();

        public LinesOfCode Loc { get; set; }

        public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
    }
}
