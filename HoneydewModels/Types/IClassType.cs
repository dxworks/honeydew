using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IClassType : IContainedType, ITypeWithModifiers, ITypeWithAttributes, ITypeWithMetrics
    {
        public string ClassType { get; set; }

        public string FilePath { get; set; }

        public IList<IBaseType> BaseTypes { get; set; }

        public IList<IImportType> Imports { get; set; }
    }
}
