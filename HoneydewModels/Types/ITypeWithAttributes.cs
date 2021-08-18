using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface ITypeWithAttributes
    {
        IList<IAttributeType> Attributes { get; set; }
    }
}
