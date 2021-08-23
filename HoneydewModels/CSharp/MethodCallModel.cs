using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record MethodCallModel : IMethodSignatureType
    {
        public string Name { get; set; }

        public string ContainingTypeName { get; set; }

        public IList<IParameterType> ParameterTypes { get; set; } = new List<IParameterType>();
    }
}
