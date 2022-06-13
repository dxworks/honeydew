namespace Honeydew.Models.Types;

public interface ITypeWithDestructor : IType
{
    public IDestructorType? Destructor { get; set; }
}
