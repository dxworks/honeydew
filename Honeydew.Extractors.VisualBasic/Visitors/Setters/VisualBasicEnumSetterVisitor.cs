using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicEnumSetterVisitor :
    CompositeVisitor<IEnumType>,
    IEnumSetterCompilationUnitVisitor<CompilationUnitSyntax, SemanticModel, EnumBlockSyntax>
{
    public VisualBasicEnumSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IEnumType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;
    public IEnumType CreateWrappedType() => new VisualBasicEnumModel();

    public IEnumerable<EnumBlockSyntax> GetWrappedSyntaxNodes(CompilationUnitSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<EnumBlockSyntax>();
    }
}
