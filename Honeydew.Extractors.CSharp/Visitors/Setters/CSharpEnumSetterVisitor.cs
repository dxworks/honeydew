using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpEnumSetterVisitor :
    CompositeVisitor<IEnumType>,
    IEnumSetterCompilationUnitVisitor<CompilationUnitSyntax, SemanticModel, EnumDeclarationSyntax>
{
    public CSharpEnumSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IEnumType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;
    public IEnumType CreateWrappedType() => new CSharpEnumModel();

    public IEnumerable<EnumDeclarationSyntax> GetWrappedSyntaxNodes(CompilationUnitSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<EnumDeclarationSyntax>();
    }
}
