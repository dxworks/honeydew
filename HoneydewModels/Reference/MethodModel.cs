using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class MethodModel : ReferenceEntity
    {
        public string Name { get; set; }

        public ClassModel Class { get; set; }

        public string MethodType { get; set; }

        public ReferenceEntity ContainingType { get; set; }

        public string Modifier { get; set; } = "";

        public string AccessModifier { get; set; }

        public ReturnValueModel ReturnValue { get; set; }

        public IList<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();

        public IList<GenericParameterModel> GenericParameters { get; set; } = new List<GenericParameterModel>();

        public IList<MethodModel> CalledMethods { get; set; } = new List<MethodModel>();
        
        public IList<ExternalMethodCall> CalledExternalMethods { get; set; } = new List<ExternalMethodCall>();

        public IList<AccessedField> AccessedFields { get; set; } = new List<AccessedField>();

        public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();

        public IList<MethodModel> LocalFunctions { get; set; } = new List<MethodModel>();

        public IList<LocalVariableModel> LocalVariables { get; set; } = new List<LocalVariableModel>();

        public LinesOfCode Loc { get; set; }

        public int CyclomaticComplexity { get; set; }

        public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
    }
}
