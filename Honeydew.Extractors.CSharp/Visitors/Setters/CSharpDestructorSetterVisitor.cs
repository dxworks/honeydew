using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpDestructorSetterVisitor :
    CompositeVisitor<IDestructorType>,
    IDestructorSetterClassVisitor<TypeDeclarationSyntax, SemanticModel, DestructorDeclarationSyntax>
{
    public CSharpDestructorSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IDestructorType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IDestructorType CreateWrappedType() => new CSharpDestructorModel();

    public IEnumerable<DestructorDeclarationSyntax> GetWrappedSyntaxNodes(TypeDeclarationSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<DestructorDeclarationSyntax>();
    }
}
