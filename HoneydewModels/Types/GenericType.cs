using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public record GenericType : INullableType
    {
        public string Name { get; set; }

        public bool IsNullable { get; set; }

        public IList<GenericType> ContainedTypes { get; set; } = new List<GenericType>();
    }
}
