using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class AttributeModel : ReferenceEntity
    {
        public string Name { get; set; }
    
        public ReferenceEntity ContainingType { get; set; }
    
        public IList<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();
    
        public string Target { get; set; }
    }
}
