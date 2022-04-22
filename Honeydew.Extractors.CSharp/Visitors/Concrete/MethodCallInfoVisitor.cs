using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class MethodCallInfoVisitor : ICSharpMethodCallVisitor
{
    public IMethodCallType Visit(InvocationExpressionSyntax syntaxNode, SemanticModel semanticModel,
        IMethodCallType modelType)
    {
        var methodCallModel = CSharpExtractionHelperMethods.GetMethodCallModel(syntaxNode, semanticModel);
        return methodCallModel;
    }
}
