namespace Honeydew.Models.Types;

public interface IFieldType : INamedType, ITypeWithModifiers, ITypeWithAttributes, ITypeWithMetrics, INullableType
{
    public IEntityType Type { get; set; }

}
