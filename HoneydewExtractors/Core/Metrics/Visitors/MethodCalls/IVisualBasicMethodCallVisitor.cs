using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.MethodCalls;

public interface IVisualBasicMethodCallVisitor : IMethodCallVisitor,
    IExtractionVisitor<InvocationExpressionSyntax, SemanticModel, IMethodSignatureType>
{
}
