using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IClassType : INamedType, ITypeWithGenericParameters, ITypeWithModifiers, ITypeWithAttributes,
        ITypeWithMetrics
    {
        public string ContainingNamespaceName { get; set; }

        public string ContainingClassName { get; set; }

        public string ClassType { get; set; }

        public string FilePath { get; set; }

        public IList<IBaseType> BaseTypes { get; set; }

        public IList<IImportType> Imports { get; set; }
    }
}
