using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface ITypeWithAttributes : IType
    {
        IList<IAttributeType> Attributes { get; set; }
    }
}
