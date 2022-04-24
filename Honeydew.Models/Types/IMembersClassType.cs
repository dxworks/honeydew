namespace Honeydew.Models.Types;

public interface IMembersClassType : IClassType, ITypeWithDestructor, ITypeWithGenericParameters
{
    public IList<IFieldType> Fields { get; init; }

    public IList<IConstructorType> Constructors { get; init; }

    public IList<IMethodType> Methods { get; init; }
}
