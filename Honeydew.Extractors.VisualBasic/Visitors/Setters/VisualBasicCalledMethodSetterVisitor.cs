using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicCalledMethodSetterVisitor :
    CompositeVisitor<IMethodCallType>,
    ICalledMethodSetterVisitor<MethodStatementSyntax, SemanticModel, InvocationExpressionSyntax, IMethodType>,
    ICalledMethodSetterVisitor<ConstructorBlockSyntax, SemanticModel, InvocationExpressionSyntax, IConstructorType>,
    ICalledMethodSetterVisitor<MethodBlockSyntax, SemanticModel, InvocationExpressionSyntax, IDestructorType>,
    ICalledMethodSetterVisitor<AccessorBlockSyntax, SemanticModel, InvocationExpressionSyntax, IAccessorMethodType>
{
    public VisualBasicCalledMethodSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IMethodCallType>> visitors)
        : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IMethodCallType CreateWrappedType() => new VisualBasicMethodCallModel();

    public IEnumerable<InvocationExpressionSyntax> GetWrappedSyntaxNodes(MethodBlockSyntax syntaxNode)
    {
        return GetInvocationExpressionSyntaxNodes(syntaxNode);
    }

    public IEnumerable<InvocationExpressionSyntax> GetWrappedSyntaxNodes(MethodStatementSyntax syntaxNode)
    {
        if (syntaxNode.Parent is MethodBlockSyntax methodBlockSyntax)
        {
            return GetInvocationExpressionSyntaxNodes(methodBlockSyntax);
        }

        return Enumerable.Empty<InvocationExpressionSyntax>();
    }

    public IEnumerable<InvocationExpressionSyntax> GetWrappedSyntaxNodes(ConstructorBlockSyntax syntaxNode)
    {
        return GetInvocationExpressionSyntaxNodes(syntaxNode);
    }

    public IEnumerable<InvocationExpressionSyntax> GetWrappedSyntaxNodes(AccessorBlockSyntax syntaxNode)
    {
        return GetInvocationExpressionSyntaxNodes(syntaxNode);
    }

    private static IEnumerable<InvocationExpressionSyntax> GetInvocationExpressionSyntaxNodes(SyntaxNode syntaxNode)
    {
        return syntaxNode.DescendantNodes()
            .OfType<InvocationExpressionSyntax>();
    }
}
