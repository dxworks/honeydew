using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public class BaseTypeModel : IModelEntity, IBaseType
    {
        public string Name { get; set; }
        
        public string ClassType { get; set; }
    }
}
