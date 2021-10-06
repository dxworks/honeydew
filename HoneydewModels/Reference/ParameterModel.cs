using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class ParameterModel : ReferenceEntity
    {
        public EntityType Type { get; set; }

        public string Modifier { get; set; } = "";

        public string DefaultValue { get; set; }

        public bool IsNullable { get; set; }
        
        public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();
        
        public ReferenceEntity ContainingType { get; set; }
    }
}
