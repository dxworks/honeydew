using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpLocalFunctionsSetterClassVisitor :
    CompositeVisitor<IMethodTypeWithLocalFunctions>,
    ILocalFunctionsSetterClassVisitor<MethodDeclarationSyntax, SemanticModel, LocalFunctionStatementSyntax,
        IMethodType>,
    ILocalFunctionsSetterClassVisitor<ConstructorDeclarationSyntax, SemanticModel, LocalFunctionStatementSyntax,
        IConstructorType>,
    ILocalFunctionsSetterClassVisitor<DestructorDeclarationSyntax, SemanticModel, LocalFunctionStatementSyntax,
        IDestructorType>,
    ILocalFunctionsSetterClassVisitor<AccessorDeclarationSyntax, SemanticModel, LocalFunctionStatementSyntax,
        IAccessorMethodType>,
    ILocalFunctionsSetterClassVisitor<LocalFunctionStatementSyntax, SemanticModel, LocalFunctionStatementSyntax,
        IMethodTypeWithLocalFunctions>
{
    public CSharpLocalFunctionsSetterClassVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IMethodTypeWithLocalFunctions>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IMethodTypeWithLocalFunctions CreateWrappedType() => new MethodModel();

    public IEnumerable<LocalFunctionStatementSyntax> GetWrappedSyntaxNodes(MethodDeclarationSyntax syntaxNode)
    {
        return syntaxNode.Body == null
            ? Enumerable.Empty<LocalFunctionStatementSyntax>()
            : syntaxNode.ChildNodes().OfType<LocalFunctionStatementSyntax>();
    }

    public IEnumerable<LocalFunctionStatementSyntax> GetWrappedSyntaxNodes(ConstructorDeclarationSyntax syntaxNode)
    {
        return syntaxNode.Body == null
            ? Enumerable.Empty<LocalFunctionStatementSyntax>()
            : syntaxNode.ChildNodes().OfType<LocalFunctionStatementSyntax>();
    }

    public IEnumerable<LocalFunctionStatementSyntax> GetWrappedSyntaxNodes(DestructorDeclarationSyntax syntaxNode)
    {
        return syntaxNode.Body == null
            ? Enumerable.Empty<LocalFunctionStatementSyntax>()
            : syntaxNode.ChildNodes().OfType<LocalFunctionStatementSyntax>();
    }

    public IEnumerable<LocalFunctionStatementSyntax> GetWrappedSyntaxNodes(AccessorDeclarationSyntax syntaxNode)
    {
        return syntaxNode.Body == null
            ? Enumerable.Empty<LocalFunctionStatementSyntax>()
            : syntaxNode.ChildNodes().OfType<LocalFunctionStatementSyntax>();
    }

    public IEnumerable<LocalFunctionStatementSyntax> GetWrappedSyntaxNodes(LocalFunctionStatementSyntax syntaxNode)
    {
        return syntaxNode.Body == null
            ? Enumerable.Empty<LocalFunctionStatementSyntax>()
            : syntaxNode.ChildNodes().OfType<LocalFunctionStatementSyntax>();
    }
}
