using System.Collections.Generic;

namespace Honeydew.Models.Types;

public interface ITypeWithLocalFunctions : IType
{
    public IList<IMethodTypeWithLocalFunctions> LocalFunctions { get; set; }
}
