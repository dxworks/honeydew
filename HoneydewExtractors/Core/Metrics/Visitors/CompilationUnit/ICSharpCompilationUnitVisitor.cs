using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;

public interface
    ICSharpCompilationUnitVisitor : ICSharpExtractionVisitor<CSharpSyntaxNode, SemanticModel, ICompilationUnitType>
{
}
