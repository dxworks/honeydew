using System.Collections.Generic;

namespace Honeydew.Models.Types;

public interface IPropertyMembersClassType : IMembersClassType
{
    public IList<IPropertyType> Properties { get; set; }
}
