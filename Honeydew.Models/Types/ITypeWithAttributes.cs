namespace Honeydew.Models.Types;

public interface ITypeWithAttributes : IType
{
    IList<IAttributeType> Attributes { get; set; }
}
