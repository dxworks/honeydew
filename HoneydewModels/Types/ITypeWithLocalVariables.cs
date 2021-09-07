using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface ITypeWithLocalVariables : IType
    {
        public IList<ILocalVariableType> LocalVariableTypes { get; set; }
    }
}
