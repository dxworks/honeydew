using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Methods
{
    public interface IVisualBasicMethodVisitor : IMethodVisitor, IVisualBasicTypeVisitor,
        IVisitorType<MethodStatementSyntax, IMethodType>
    {
    }
}
