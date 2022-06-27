namespace Honeydew.Models.Types;

public interface IMethodCallType : IMethodSignatureType
{
    public string DefinitionClassName { get; set; } // base class

    public string LocationClassName { get; set; } // derived class

    public IList<string> MethodDefinitionNames { get; set; }

    public IList<IEntityType> GenericParameters { get; set; }
}
