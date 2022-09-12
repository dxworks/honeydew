using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpAccessedFieldsSetterVisitor :
    CompositeVisitor<AccessedField?>,
    IAccessedFieldsSetterVisitor<MethodDeclarationSyntax, SemanticModel, ExpressionSyntax, IMethodType>,
    IAccessedFieldsSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, ExpressionSyntax, IConstructorType>,
    IAccessedFieldsSetterVisitor<DestructorDeclarationSyntax, SemanticModel, ExpressionSyntax, IDestructorType>,
    IAccessedFieldsSetterVisitor<AccessorDeclarationSyntax, SemanticModel, ExpressionSyntax, IAccessorMethodType>,
    IAccessedFieldsSetterVisitor<ArrowExpressionClauseSyntax, SemanticModel, ExpressionSyntax, IAccessorMethodType>,
    IAccessedFieldsSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, ExpressionSyntax,
        IMethodTypeWithLocalFunctions>
{
    public CSharpAccessedFieldsSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<AccessedField?>> visitors)
        : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public AccessedField CreateWrappedType() => new();

    public IEnumerable<ExpressionSyntax> GetWrappedSyntaxNodes(MethodDeclarationSyntax syntaxNode)
    {
        var descendantNodes = syntaxNode.DescendantNodes().ToList();
        var possibleMemberAccessExpressions = GetPossibleAccessFields(descendantNodes);

        return possibleMemberAccessExpressions.Where(memberAccessExpressionSyntax =>
            memberAccessExpressionSyntax.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>() == null);
    }

    public IEnumerable<ExpressionSyntax> GetWrappedSyntaxNodes(ConstructorDeclarationSyntax syntaxNode)
    {
        var descendantNodes = syntaxNode.DescendantNodes().ToList();
        var possibleMemberAccessExpressions = GetPossibleAccessFields(descendantNodes);

        return possibleMemberAccessExpressions.Where(memberAccessExpressionSyntax =>
            memberAccessExpressionSyntax.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>() == null);
    }

    public IEnumerable<ExpressionSyntax> GetWrappedSyntaxNodes(DestructorDeclarationSyntax syntaxNode)
    {
        var descendantNodes = syntaxNode.DescendantNodes().ToList();
        var possibleMemberAccessExpressions = GetPossibleAccessFields(descendantNodes);

        return possibleMemberAccessExpressions.Where(memberAccessExpressionSyntax =>
            memberAccessExpressionSyntax.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>() == null);
    }

    public IEnumerable<ExpressionSyntax> GetWrappedSyntaxNodes(AccessorDeclarationSyntax syntaxNode)
    {
        var descendantNodes = syntaxNode.DescendantNodes().ToList();
        var possibleMemberAccessExpressions = GetPossibleAccessFields(descendantNodes);

        return possibleMemberAccessExpressions.Where(memberAccessExpressionSyntax =>
            memberAccessExpressionSyntax.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>() == null);
    }

    public IEnumerable<ExpressionSyntax> GetWrappedSyntaxNodes(ArrowExpressionClauseSyntax syntaxNode)
    {
        var descendantNodes = syntaxNode.DescendantNodes().ToList();
        var possibleMemberAccessExpressions = GetPossibleAccessFields(descendantNodes);

        return possibleMemberAccessExpressions.Where(memberAccessExpressionSyntax =>
            memberAccessExpressionSyntax.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>() == null);
    }

    public IEnumerable<ExpressionSyntax> GetWrappedSyntaxNodes(LocalFunctionStatementSyntax syntaxNode)
    {
        if (syntaxNode.Body == null)
        {
            return Enumerable.Empty<ExpressionSyntax>();
        }

        var descendantNodes = syntaxNode.Body.ChildNodes().ToList();
        var possibleMemberAccessExpressions = descendantNodes.OfType<LocalDeclarationStatementSyntax>()
            .SelectMany(syntax => GetPossibleAccessFields(syntax.DescendantNodes().ToList()));

        possibleMemberAccessExpressions = possibleMemberAccessExpressions.Concat(descendantNodes
            .OfType<ExpressionStatementSyntax>()
            .SelectMany(syntax => GetPossibleAccessFields(syntax.DescendantNodes().ToList())));

        return possibleMemberAccessExpressions;
    }

    private static IEnumerable<ExpressionSyntax> GetPossibleAccessFields(IEnumerable<SyntaxNode> descendantNodes)
    {
        return descendantNodes.OfType<IdentifierNameSyntax>();
    }
}
