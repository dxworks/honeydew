using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpConstructorSetterVisitor :
    CompositeVisitor<IConstructorType>,
    IConstructorSetterClassVisitor<TypeDeclarationSyntax, SemanticModel, ConstructorDeclarationSyntax>
{
    public CSharpConstructorSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IConstructorType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IConstructorType CreateWrappedType() => new CSharpConstructorModel();

    public IEnumerable<ConstructorDeclarationSyntax> GetWrappedSyntaxNodes(TypeDeclarationSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
    }
}
