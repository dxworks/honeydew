using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures
{
    public interface ICSharpMethodSignatureVisitor : IMethodSignatureVisitor, ICSharpTypeVisitor,
        IVisitorType<InvocationExpressionSyntax, IMethodSignatureType>
    {
    }
}
