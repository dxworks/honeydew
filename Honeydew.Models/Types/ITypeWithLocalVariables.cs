using System.Collections.Generic;

namespace Honeydew.Models.Types;

public interface ITypeWithLocalVariables : IType
{
    public IList<ILocalVariableType> LocalVariableTypes { get; set; }
}
