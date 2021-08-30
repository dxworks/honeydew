using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IPropertyMembersClassType : IMembersClassType
    {
        public IList<IPropertyType> Properties { get; set; }
    }
}
