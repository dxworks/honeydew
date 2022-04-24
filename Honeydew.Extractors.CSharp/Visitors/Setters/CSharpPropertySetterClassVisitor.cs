using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpPropertySetterClassVisitor :
    CompositeVisitor<IPropertyType>,
    IPropertySetterClassVisitor<TypeDeclarationSyntax, SemanticModel, BasePropertyDeclarationSyntax>
{
    public CSharpPropertySetterClassVisitor(ILogger logger, IEnumerable<ITypeVisitor<IPropertyType>> visitors) : base(
        logger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IPropertyType CreateWrappedType() => new PropertyModel();

    public IEnumerable<BasePropertyDeclarationSyntax> GetWrappedSyntaxNodes(TypeDeclarationSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<BasePropertyDeclarationSyntax>();
    }
}
