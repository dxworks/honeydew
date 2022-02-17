using System.Collections.Generic;

namespace HoneydewModels.Types;

public interface ICallingMethodsType : INamedType
{
    public IList<IMethodCallType> CalledMethods { get; set; }
}
