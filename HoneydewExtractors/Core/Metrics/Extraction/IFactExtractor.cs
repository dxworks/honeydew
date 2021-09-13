using HoneydewModels.Types;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.Core.Metrics.Extraction
{
    public interface IFactExtractor
    {
        ICompilationUnitType Extract(SyntaxTree syntacticTree, SemanticModel semanticModel);
    }
}
