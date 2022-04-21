namespace Honeydew.Models.Types;

public interface IReturnValueType : ITypeWithAttributes, INullableType
{
    public IEntityType Type { get; set; }
}
