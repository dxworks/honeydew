namespace Honeydew.Models.Types;

public interface IParameterType : ITypeWithAttributes, INullableType
{
    public IEntityType Type { get; set; }
    
    public string Modifier { get; set; }
}
