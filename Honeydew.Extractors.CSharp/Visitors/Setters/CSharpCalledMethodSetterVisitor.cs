using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpCalledMethodSetterVisitor :
    CompositeVisitor<IMethodCallType>,
    ICalledMethodSetterVisitor<MethodDeclarationSyntax, SemanticModel, InvocationExpressionSyntax, IMethodType>,
    ICalledMethodSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, InvocationExpressionSyntax,
        IConstructorType>,
    ICalledMethodSetterVisitor<DestructorDeclarationSyntax, SemanticModel, InvocationExpressionSyntax, IDestructorType>,
    ICalledMethodSetterVisitor<AccessorDeclarationSyntax, SemanticModel, InvocationExpressionSyntax,
        IAccessorMethodType>,
    ICalledMethodSetterVisitor<ArrowExpressionClauseSyntax, SemanticModel, InvocationExpressionSyntax,
        IAccessorMethodType>,
    ICalledMethodSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, InvocationExpressionSyntax,
        IMethodTypeWithLocalFunctions>
{
    public CSharpCalledMethodSetterVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<IMethodCallType>> visitors)
        : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IMethodCallType CreateWrappedType() => new MethodCallModel();

    public IEnumerable<InvocationExpressionSyntax> GetWrappedSyntaxNodes(MethodDeclarationSyntax syntaxNode)
    {
        return GetInvocationExpressionSyntaxNodes(syntaxNode);
    }

    public IEnumerable<InvocationExpressionSyntax> GetWrappedSyntaxNodes(ConstructorDeclarationSyntax syntaxNode)
    {
        return GetInvocationExpressionSyntaxNodes(syntaxNode);
    }

    public IEnumerable<InvocationExpressionSyntax> GetWrappedSyntaxNodes(DestructorDeclarationSyntax syntaxNode)
    {
        return GetInvocationExpressionSyntaxNodes(syntaxNode);
    }

    public IEnumerable<InvocationExpressionSyntax> GetWrappedSyntaxNodes(AccessorDeclarationSyntax syntaxNode)
    {
        return GetInvocationExpressionSyntaxNodes(syntaxNode);
    }

    public IEnumerable<InvocationExpressionSyntax> GetWrappedSyntaxNodes(ArrowExpressionClauseSyntax syntaxNode)
    {
        return GetInvocationExpressionSyntaxNodes(syntaxNode);
    }

    public IEnumerable<InvocationExpressionSyntax> GetWrappedSyntaxNodes(LocalFunctionStatementSyntax syntaxNode)
    {
        if (syntaxNode.Body == null)
        {
            return Enumerable.Empty<InvocationExpressionSyntax>();
        }

        var invocationExpressionSyntaxNodes =
            syntaxNode.Body.ChildNodes().OfType<InvocationExpressionSyntax>().ToList();

        foreach (var returnStatementSyntax in syntaxNode.Body.ChildNodes().OfType<ReturnStatementSyntax>())
        {
            invocationExpressionSyntaxNodes.AddRange(returnStatementSyntax.DescendantNodes()
                .OfType<InvocationExpressionSyntax>());
        }

        foreach (var awaitExpressionSyntax in syntaxNode.Body.ChildNodes().OfType<AwaitExpressionSyntax>())
        {
            invocationExpressionSyntaxNodes.AddRange(awaitExpressionSyntax.DescendantNodes()
                .OfType<InvocationExpressionSyntax>());
        }

        foreach (var awaitExpressionSyntax in
                 syntaxNode.Body.ChildNodes().OfType<LocalDeclarationStatementSyntax>())
        {
            invocationExpressionSyntaxNodes.AddRange(awaitExpressionSyntax.DescendantNodes()
                .OfType<InvocationExpressionSyntax>());
        }

        return invocationExpressionSyntaxNodes;
    }

    private static IEnumerable<InvocationExpressionSyntax> GetInvocationExpressionSyntaxNodes(SyntaxNode syntaxNode)
    {
        return syntaxNode.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(invocationExpressionSyntax =>
                invocationExpressionSyntax.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>() == null);
    }
}
