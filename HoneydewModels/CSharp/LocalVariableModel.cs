using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record LocalVariableModel : ILocalVariableType
    {
        public IEntityType Type { get; set; }
    }
}
