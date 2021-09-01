using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record LocalVariableModel : IModelEntity, ILocalVariableType
    {
        public IEntityType Type { get; set; }
    }
}
