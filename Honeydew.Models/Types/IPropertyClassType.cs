namespace Honeydew.Models.Types;

public interface IPropertyMembersClassType : IMembersClassType
{
    public IList<IPropertyType> Properties { get; set; }
}
