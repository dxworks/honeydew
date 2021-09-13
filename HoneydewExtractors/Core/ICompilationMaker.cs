using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.Core
{
    public interface ICompilationMaker
    {
        Compilation GetCompilation();
    }
}
