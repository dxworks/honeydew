using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.MethodCall;

public class MethodCallInfoVisitor : ICSharpMethodSignatureVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IMethodSignatureType Visit(InvocationExpressionSyntax syntaxNode, SemanticModel semanticModel,
        IMethodSignatureType modelType)
    {
        var methodCallModel = CSharpExtractionHelperMethods.GetMethodCallModel(syntaxNode, semanticModel);
        return methodCallModel;
    }
}
