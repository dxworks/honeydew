namespace Honeydew.Models.Types;

public interface IEventType : INamedType, ITypeWithModifiers
{
    public bool IsEvent { get; set; }
}
