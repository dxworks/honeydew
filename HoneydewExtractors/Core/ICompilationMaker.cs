using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.Core
{
    public interface ICompilationMaker
    {
        Compilation GetCompilation();

        IEnumerable<MetadataReference> FindTrustedReferences();
    }
}
