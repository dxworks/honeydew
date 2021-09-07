using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public record GenericType
    {
        public string Name { get; set; }

        public IList<GenericType> ContainedTypes { get; set; } = new List<GenericType>();
    }
}
