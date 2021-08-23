using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IMethodSignatureType : IContainedType
    {
        public IList<IParameterType> ParameterTypes { get; set; }
    }
}
