using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp;

public record MethodCallModel : IMethodCallType
{
    public string Name { get; set; }

    public string DefinitionClassName { get; set; }

    public string LocationClassName { get; set; }

    public IList<string> DefinitionMethodNames { get; set; } = new List<string>();

    public IList<IParameterType> ParameterTypes { get; set; } = new List<IParameterType>();
}
