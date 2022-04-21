using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.MethodCalls;

public interface ICSharpMethodCallVisitor : IMethodCallVisitor,
    IExtractionVisitor<InvocationExpressionSyntax, SemanticModel, IMethodCallType>
{
}
