using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicInterfaceSetterVisitor :
    CompositeVisitor<IMembersClassType>,
    IClassSetterCompilationUnitVisitor<CompilationUnitSyntax, SemanticModel, InterfaceStatementSyntax>
{
    public VisualBasicInterfaceSetterVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<IMembersClassType>> visitors)
        : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IMembersClassType CreateWrappedType() => new VisualBasicClassModel();

    public IEnumerable<InterfaceStatementSyntax> GetWrappedSyntaxNodes(CompilationUnitSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<InterfaceStatementSyntax>();
    }
}
