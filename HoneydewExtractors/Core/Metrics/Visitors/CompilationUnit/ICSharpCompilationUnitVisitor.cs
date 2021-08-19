using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp;

namespace HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit
{
    public interface ICSharpCompilationUnitVisitor : ICompilationUnitVisitor, ICSharpTypeVisitor,
        IVisitorType<CSharpSyntaxNode, ICompilationUnitType>
    {
    }
}
