using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpMethodSetterVisitor :
    CompositeVisitor<IMethodType>,
    IMethodSetterClassVisitor<TypeDeclarationSyntax, SemanticModel, MethodDeclarationSyntax>
{
    public CSharpMethodSetterVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<IMethodType>> visitors) :
        base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IMethodType CreateWrappedType() => new CSharpMethodModel();

    public IEnumerable<MethodDeclarationSyntax> GetWrappedSyntaxNodes(TypeDeclarationSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<MethodDeclarationSyntax>();
    }
}
