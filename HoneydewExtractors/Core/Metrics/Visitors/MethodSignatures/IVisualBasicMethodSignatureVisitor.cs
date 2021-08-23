using HoneydewModels.Types;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures
{
    public interface IVisualBasicMethodSignatureVisitor : IMethodSignatureVisitor,
        IExtractionVisitor<InvocationExpressionSyntax, IMethodSignatureType>
    {
    }
}
