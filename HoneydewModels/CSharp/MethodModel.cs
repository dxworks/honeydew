using System.Collections.Generic;

namespace HoneydewModels.CSharp
{
    public record MethodModel
    {
        public string Name { get; init; }

        public LinesOfCode Loc { get; set; }

        public int CyclomaticComplexity { get; set; }

        public bool IsConstructor { get; init; } = false;

        public ReturnTypeModel ReturnType { get; set; }

        public string Modifier { get; init; } = "";
        public string AccessModifier { get; init; }
        public IList<ParameterModel> ParameterTypes { get; } = new List<ParameterModel>();

        public string ContainingClassName { get; set; }
        public IList<MethodCallModel> CalledMethods { get; } = new List<MethodCallModel>();
    }
}
