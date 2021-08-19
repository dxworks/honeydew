using HoneydewModels.Types;
using Microsoft.CodeAnalysis.VisualBasic;

namespace HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit
{
    public interface IVisualBasicCompilationUnitVisitor : ICompilationUnitVisitor, IVisualBasicTypeVisitor,
        IVisitorType<VisualBasicSyntaxNode, ICompilationUnitType>
    {
    }
}
