using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface ITypeWithGenericParameters : IType
    {
        public IList<IGenericParameterType> GenericParameters { get; set; }
    }
}
