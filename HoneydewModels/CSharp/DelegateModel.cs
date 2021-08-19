using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public class DelegateModel : IDelegateType, IModelEntity
    {
        public string ClassType { get; init; }

        public string FilePath { get; set; }

        public string Name { get; set; }

        public IList<IBaseType> BaseTypes { get; set; }

        public IList<IImportType> Imports { get; set; }

        public string ContainingTypeName { get; set; }

        public string AccessModifier { get; set; }

        public string Modifier { get; set; }

        public IList<IAttributeType> Attributes { get; set; }

        public IList<IParameterType> ParameterTypes { get; set; }

        public IReturnType ReturnType { get; set; }

        public IList<MetricModel> Metrics { get; init; }
    }
}
