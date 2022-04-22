using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;

namespace Honeydew.Extractors.Metrics;

public interface IFactExtractor
{
    ICompilationUnitType Extract(SyntaxTree syntacticTree, SemanticModel semanticModel);
}
