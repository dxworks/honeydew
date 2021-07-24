using System.Collections.Generic;

namespace HoneydewModels.CSharp
{
    public record MethodCallModel
    {
        public string MethodName { get; init; }

        public string ContainingClassName { get; set; }

        public IList<ParameterModel> ParameterTypes { get; init; } = new List<ParameterModel>();
    }
}
