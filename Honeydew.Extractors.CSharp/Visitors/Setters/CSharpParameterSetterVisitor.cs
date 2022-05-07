using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpParameterSetterVisitor :
    CompositeVisitor<IParameterType>,
    IParameterSetterVisitor<DelegateDeclarationSyntax, SemanticModel, ParameterSyntax, IDelegateType>,
    IParameterSetterVisitor<MethodDeclarationSyntax, SemanticModel, ParameterSyntax, IMethodType>,
    IParameterSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, ParameterSyntax, IConstructorType>,
    IParameterSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, ParameterSyntax, IMethodTypeWithLocalFunctions>
{
    public CSharpParameterSetterVisitor(ILogger logger, IEnumerable<ITypeVisitor<IParameterType>> visitors) : base(
        logger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IParameterType CreateWrappedType() => new CSharpParameterModel();

    public IEnumerable<ParameterSyntax> GetWrappedSyntaxNodes(DelegateDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<ParameterListSyntax>()
            .SelectMany(syntax => syntax.Parameters);
    }

    public IEnumerable<ParameterSyntax> GetWrappedSyntaxNodes(MethodDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<ParameterListSyntax>()
            .SelectMany(syntax => syntax.Parameters);
    }

    public IEnumerable<ParameterSyntax> GetWrappedSyntaxNodes(ConstructorDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<ParameterListSyntax>()
            .SelectMany(syntax => syntax.Parameters);
    }

    public IEnumerable<ParameterSyntax> GetWrappedSyntaxNodes(LocalFunctionStatementSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<ParameterListSyntax>()
            .SelectMany(syntax => syntax.Parameters);
    }
}
