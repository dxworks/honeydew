using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpDestructorSetterClassVisitor :
    CompositeVisitor<IDestructorType>,
    IDestructorSetterClassVisitor<TypeDeclarationSyntax, SemanticModel, DestructorDeclarationSyntax>
{
    public CSharpDestructorSetterClassVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IDestructorType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IDestructorType CreateWrappedType() => new DestructorModel();

    public IEnumerable<DestructorDeclarationSyntax> GetWrappedSyntaxNodes(TypeDeclarationSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<DestructorDeclarationSyntax>();
    }
}
