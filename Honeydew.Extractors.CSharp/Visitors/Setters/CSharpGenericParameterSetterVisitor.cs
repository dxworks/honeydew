using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpGenericParameterSetterVisitor :
    CompositeVisitor<IGenericParameterType>,
    IGenericParameterSetterVisitor<TypeDeclarationSyntax, SemanticModel, TypeParameterSyntax, IMembersClassType>,
    IGenericParameterSetterVisitor<DelegateDeclarationSyntax, SemanticModel, TypeParameterSyntax, IDelegateType>,
    IGenericParameterSetterVisitor<MethodDeclarationSyntax, SemanticModel, TypeParameterSyntax, IMethodType>,
    IGenericParameterSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, TypeParameterSyntax,
        IMethodTypeWithLocalFunctions>
{
    public CSharpGenericParameterSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IGenericParameterType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IGenericParameterType CreateWrappedType() => new CSharpGenericParameterModel();

    public IEnumerable<TypeParameterSyntax> GetWrappedSyntaxNodes(TypeDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<TypeParameterListSyntax>()
            .SelectMany(listSyntax => listSyntax.Parameters);
    }

    public IEnumerable<TypeParameterSyntax> GetWrappedSyntaxNodes(DelegateDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<TypeParameterListSyntax>()
            .SelectMany(listSyntax => listSyntax.Parameters);
    }

    public IEnumerable<TypeParameterSyntax> GetWrappedSyntaxNodes(MethodDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<TypeParameterListSyntax>()
            .SelectMany(listSyntax => listSyntax.Parameters);
    }

    public IEnumerable<TypeParameterSyntax> GetWrappedSyntaxNodes(LocalFunctionStatementSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<TypeParameterListSyntax>()
            .SelectMany(listSyntax => listSyntax.Parameters);
    }
}
