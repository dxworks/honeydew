using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;

public interface IVisualBasicCompilationUnitVisitor : ICompilationUnitVisitor,
    IExtractionVisitor<VisualBasicSyntaxNode, SemanticModel, ICompilationUnitType>
{
}
