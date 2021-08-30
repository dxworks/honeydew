using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface ICallingMethodsType : IContainedType
    {
        public IList<IMethodSignatureType> CalledMethods { get; set; }
    }
}
