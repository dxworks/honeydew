namespace Honeydew.Models.Types;

public interface IContainedTypeWithAccessedFields : INamedType
{
    public IList<AccessedField> AccessedFields { get; set; }
}
