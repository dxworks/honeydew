using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public class EntityTypeModel : IEntityType
    {
        public string Name { get; set; }

        public IList<GenericType> ContainedTypes { get; set; } = new List<GenericType>();
    }
}
