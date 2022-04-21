using System.Collections.Generic;

namespace Honeydew.Models.Types;

public interface ITypeWithGenericParameters : IType
{
    public IList<IGenericParameterType> GenericParameters { get; set; }
}
