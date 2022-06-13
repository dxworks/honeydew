using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicDelegateSetterVisitor :
    CompositeVisitor<IDelegateType>,
    IDelegateSetterCompilationUnitVisitor<CompilationUnitSyntax, SemanticModel, DelegateStatementSyntax>
{
    public VisualBasicDelegateSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IDelegateType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IDelegateType CreateWrappedType() => new VisualBasicDelegateModel();

    public IEnumerable<DelegateStatementSyntax> GetWrappedSyntaxNodes(CompilationUnitSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<DelegateStatementSyntax>();
    }
}
