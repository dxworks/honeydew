using Honeydew.Extractors.Dotnet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Honeydew.Extractors.CSharp;

public class CSharpCompilationMaker : DotnetCompilationMaker
{
    protected override Compilation GetConcreteCompilation()
    {
        return CSharpCompilation.Create("Compilation", references: References,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
