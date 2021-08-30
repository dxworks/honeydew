using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public class BaseTypeModel : IModelEntity, IBaseType
    {
        public IEntityType Type { get; set; }

        public string Kind { get; set; }
    }
}
