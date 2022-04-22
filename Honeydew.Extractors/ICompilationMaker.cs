using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Honeydew.Extractors;

public interface ICompilationMaker
{
    Compilation GetCompilation();

    IEnumerable<MetadataReference> FindTrustedReferences();
}
