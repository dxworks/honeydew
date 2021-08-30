using HoneydewModels.Types;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Methods
{
    public interface IVisualBasicMethodVisitor : IMethodVisitor,
        IExtractionVisitor<MethodStatementSyntax, IMethodType>
    {
    }
}
