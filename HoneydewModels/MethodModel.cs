using System.Collections.Generic;

namespace HoneydewModels
{
    public record MethodModel
    {
        public string Name { get; init; }

        public bool IsConstructor { get; init; } = false;

        public string ReturnType { get; set; }

        public string Modifier { get; init; } = "";
        public string AccessModifier { get; init; }
        public IList<ParameterModel> ParameterTypes { get; } = new List<ParameterModel>();

        public string ContainingClassName { get; set; }
        public IList<MethodCallModel> CalledMethods { get; } = new List<MethodCallModel>();
    }
}
