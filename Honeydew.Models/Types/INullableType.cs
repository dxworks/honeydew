namespace Honeydew.Models.Types;

public interface INullableType : IType
{
    public bool IsNullable { get; set; }
}
