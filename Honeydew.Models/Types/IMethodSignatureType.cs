namespace Honeydew.Models.Types;

public interface IMethodSignatureType : INamedType
{
    public IList<IParameterType> ParameterTypes { get; set; }
}
