using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class GenericParameterModel : ReferenceEntity
    {
        public string Name { get; set; }

        public string Modifier { get; set; }

        public IList<EntityType> Constraints { get; set; } = new List<EntityType>();

        public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();
    }
}
