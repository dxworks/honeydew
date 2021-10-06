using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class ReturnValueModel : ReferenceEntity
    {
        public EntityType Type { get; set; }

        public string Modifier { get; set; } = "";

        public bool IsNullable { get; set; }

        public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();

        public ReferenceEntity ContainingType { get; set; }
    }
}
