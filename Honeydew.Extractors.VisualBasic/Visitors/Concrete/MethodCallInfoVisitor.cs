using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Utils;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class MethodCallInfoVisitor : IExtractionVisitor<InvocationExpressionSyntax, SemanticModel, IMethodCallType>
{
    public IMethodCallType Visit(InvocationExpressionSyntax syntaxNode, SemanticModel semanticModel,
        IMethodCallType modelType)
    {
        var methodCallModel = VisualBasicExtractionHelperMethods.GetMethodCallModel(syntaxNode, semanticModel);
        return methodCallModel;
    }
}
