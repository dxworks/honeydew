using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpEnumSetterCompilationUnitVisitor :
    CompositeVisitor<IEnumType>,
    IEnumSetterCompilationUnitVisitor<CSharpSyntaxNode, SemanticModel, EnumDeclarationSyntax>
{
    public CSharpEnumSetterCompilationUnitVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IEnumType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;
    public IEnumType CreateWrappedType() => new EnumModel();

    public IEnumerable<EnumDeclarationSyntax> GetWrappedSyntaxNodes(CSharpSyntaxNode syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<EnumDeclarationSyntax>();
    }
}
