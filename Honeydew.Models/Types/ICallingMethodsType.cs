using System.Collections.Generic;

namespace Honeydew.Models.Types;

public interface ICallingMethodsType : INamedType
{
    public IList<IMethodCallType> CalledMethods { get; set; }
}
