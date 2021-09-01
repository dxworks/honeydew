using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface ITypeWithLocalVariables : IType
    {
        public IList<IEntityType> LocalVariableTypes { get; set; }
    }
}
