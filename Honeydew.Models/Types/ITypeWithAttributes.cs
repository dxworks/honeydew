using System.Collections.Generic;

namespace Honeydew.Models.Types;

public interface ITypeWithAttributes : IType
{
    IList<IAttributeType> Attributes { get; set; }
}
