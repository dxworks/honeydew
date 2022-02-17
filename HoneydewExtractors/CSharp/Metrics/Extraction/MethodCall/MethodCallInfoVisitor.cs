using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.MethodCalls;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.MethodCall;

public class MethodCallInfoVisitor : ICSharpMethodCallVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IMethodCallType Visit(InvocationExpressionSyntax syntaxNode, SemanticModel semanticModel,
        IMethodCallType modelType)
    {
        var methodCallModel = CSharpExtractionHelperMethods.GetMethodCallModel(syntaxNode, semanticModel);
        return methodCallModel;
    }
}
