using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public record GenericType
    {
        public ReferenceEntity Reference { get; set; }

        public bool IsNullable { get; set; }

        public IList<GenericType> ContainedTypes { get; set; } = new List<GenericType>();
    }
}
