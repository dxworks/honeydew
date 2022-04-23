using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpDelegateSetterCompilationUnitVisitor :
    CompositeVisitor<IDelegateType>,
    IDelegateSetterCompilationUnitVisitor<CompilationUnitSyntax, SemanticModel, DelegateDeclarationSyntax>
{
    public CSharpDelegateSetterCompilationUnitVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IDelegateType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IDelegateType CreateWrappedType() => new DelegateModel();

    public IEnumerable<DelegateDeclarationSyntax> GetWrappedSyntaxNodes(CompilationUnitSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<DelegateDeclarationSyntax>();
    }
}
