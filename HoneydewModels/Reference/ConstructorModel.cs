using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class ConstructorModel : ReferenceEntity
    {
        public string Name { get; set; }

        public ClassModel Class { get; set; }

        public string AccessModifier { get; set; }

        public string Modifier { get; set; }

        public IList<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();

        public IList<MethodCallModel> CalledMethods { get; set; } = new List<MethodCallModel>();

        public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();

        public IList<MethodModel> LocalFunctions { get; set; } = new List<MethodModel>();

        public IList<LocalVariableModel> LocalVariables { get; set; } = new List<LocalVariableModel>();

        public LinesOfCode Loc { get; set; }

        public int CyclomaticComplexity { get; set; }

        public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
    }
}
