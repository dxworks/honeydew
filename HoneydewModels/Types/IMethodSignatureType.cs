using System.Collections.Generic;

namespace HoneydewModels.Types;

public interface IMethodSignatureType : INamedType
{
    public IList<IParameterType> ParameterTypes { get; set; }
}
