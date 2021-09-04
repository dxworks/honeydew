using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IGenericParameterType : INamedType, ITypeWithAttributes
    {
        public string Modifier { get; set; }

        public IList<IEntityType> Constraints { get; set; }
    }
}
