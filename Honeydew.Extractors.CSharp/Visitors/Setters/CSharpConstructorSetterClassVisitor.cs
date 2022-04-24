using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpConstructorSetterClassVisitor :
    CompositeVisitor<IConstructorType>,
    IConstructorSetterClassVisitor<TypeDeclarationSyntax, SemanticModel, ConstructorDeclarationSyntax>
{
    public CSharpConstructorSetterClassVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IConstructorType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IConstructorType CreateWrappedType() => new ConstructorModel();

    public IEnumerable<ConstructorDeclarationSyntax> GetWrappedSyntaxNodes(TypeDeclarationSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
    }
}
