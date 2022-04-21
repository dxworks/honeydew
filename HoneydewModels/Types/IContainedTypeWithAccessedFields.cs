using System.Collections.Generic;

namespace HoneydewModels.Types;

public interface IContainedTypeWithAccessedFields : INamedType
{
    public IList<AccessedField> AccessedFields { get; set; }
}
