using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface ITypeWithLocalFunctions : IType
    {
        public IList<IMethodTypeWithLocalFunctions> LocalFunctions { get; set; }
    }
}
