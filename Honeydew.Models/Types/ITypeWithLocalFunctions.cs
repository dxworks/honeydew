namespace Honeydew.Models.Types;

public interface ITypeWithLocalFunctions : IType
{
    public IList<IMethodTypeWithLocalFunctions> LocalFunctions { get; set; }
}
