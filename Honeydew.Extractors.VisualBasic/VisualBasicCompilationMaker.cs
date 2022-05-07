using Honeydew.Extractors.Dotnet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Honeydew.Extractors.VisualBasic;

public class VisualBasicCompilationMaker : DotnetCompilationMaker
{
    protected override Compilation GetConcreteCompilation()
    {
        return  VisualBasicCompilation.Create("Compilation", references: References,
            options: new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
