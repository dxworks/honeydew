using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.Core
{
    public interface ICompilationMaker
    {
        Compilation GetCompilation();

        void AddReference(Compilation compilation, string referencePath);
    }
}
