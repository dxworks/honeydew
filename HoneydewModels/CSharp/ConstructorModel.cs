using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public class ConstructorModel : IModelEntity, IConstructorType
    {
        public string Name { get; set; }

        public string ContainingTypeName { get; set; }

        public string AccessModifier { get; set; }

        public string Modifier { get; set; }

        public IList<IParameterType> ParameterTypes { get; set; } = new List<IParameterType>();
        public IList<IMethodSignatureType> CalledMethods { get; set; } = new List<IMethodSignatureType>();

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();

        public LinesOfCode Loc { get; set; }
        public int CyclomaticComplexity { get; set; }
    }
}
