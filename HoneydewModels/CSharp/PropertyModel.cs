using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record PropertyModel : IModelEntity, IPropertyType
    {
        public int CyclomaticComplexity { get; set; }
        public string Name { get; set; }

        public string ContainingTypeName { get; set; }

        public string AccessModifier { get; init; }

        public string Modifier { get; init; } = "";

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();

        public string Type { get; set; }

        public bool IsEvent { get; init; }

        public IList<IMethodSignatureType> CalledMethods { get; set; } = new List<IMethodSignatureType>();

        public IList<string> Accessors { get; set; } = new List<string>();

        public LinesOfCode Loc { get; set; }
    }
}
