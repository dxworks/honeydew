using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface ITypeWithLocalFunctions
    {
        public IList<IMethodTypeWithLocalFunctions> LocalFunctions { get; set; }
    }
}
