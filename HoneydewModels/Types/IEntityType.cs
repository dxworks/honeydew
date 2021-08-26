using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IEntityType : INamedType
    {
        public IList<GenericType> ContainedTypes { get; set; }
    }
}   
