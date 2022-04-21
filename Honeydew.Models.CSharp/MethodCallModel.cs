using Honeydew.Models.Types;

namespace Honeydew.Models.CSharp;

public record MethodCallModel : IMethodCallType
{
    public string Name { get; set; }

    public string DefinitionClassName { get; set; }

    public string LocationClassName { get; set; }

    public IList<string> MethodDefinitionNames { get; set; } = new List<string>();

    public IList<IParameterType> ParameterTypes { get; set; } = new List<IParameterType>();

    public IList<IEntityType> GenericParameters { get; set; } = new List<IEntityType>();
}
