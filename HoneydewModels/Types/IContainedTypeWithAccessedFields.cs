using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IContainedTypeWithAccessedFields : IContainedType
    {
        public IList<AccessedField> AccessedFields { get; set; }
    }
}
