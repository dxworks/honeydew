using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public record GenericType
    {
        public string Name { get; set; }

        public string Modifier { get; set; }

        public IList<GenericType> ContainedTypes { get; set; } = new List<GenericType>();

        public IList<GenericType> Constrains { get; set; } = new List<GenericType>();
    }
}
