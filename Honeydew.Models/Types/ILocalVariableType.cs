namespace Honeydew.Models.Types;

public interface ILocalVariableType : INullableType, INamedType
{
    public IEntityType Type { get; set; }

    public string Modifier { get; set; }
}
