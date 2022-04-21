using System.Collections.Generic;

namespace Honeydew.Models.Types;

public interface IContainedTypeWithAccessedFields : INamedType
{
    public IList<AccessedField> AccessedFields { get; set; }
}
