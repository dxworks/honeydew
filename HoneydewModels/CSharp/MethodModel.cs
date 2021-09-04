using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record MethodModel : IModelEntity, IMethodTypeWithLocalFunctions
    {
        public string Name { get; set; }

        public string ContainingTypeName { get; set; }

        public string Modifier { get; set; } = "";

        public string AccessModifier { get; set; }

        public IReturnValueType ReturnValue { get; set; }

        public IList<IParameterType> ParameterTypes { get; set; } = new List<IParameterType>();

        public IList<IGenericParameterType> GenericParameters { get; set; } = new List<IGenericParameterType>();

        public IList<IMethodSignatureType> CalledMethods { get; set; } = new List<IMethodSignatureType>();

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();

        public IList<IMethodTypeWithLocalFunctions> LocalFunctions { get; set; } =
            new List<IMethodTypeWithLocalFunctions>();

        public IList<ILocalVariableType> LocalVariableTypes { get; set; } = new List<ILocalVariableType>();

        public LinesOfCode Loc { get; set; }

        public int CyclomaticComplexity { get; set; }

        public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
    }
}
