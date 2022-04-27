using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicEnumLabelsSetterVisitor :
    CompositeVisitor<IEnumLabelType>,
    IEnumLabelsSetterVisitor<EnumBlockSyntax, SemanticModel, EnumMemberDeclarationSyntax>
{
    public VisualBasicEnumLabelsSetterVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<IEnumLabelType>> visitors) :
        base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IEnumLabelType CreateWrappedType() => new VisualBasicEnumLabelType();

    public IEnumerable<EnumMemberDeclarationSyntax> GetWrappedSyntaxNodes(EnumBlockSyntax syntaxNode)
    {
        return syntaxNode.Members.OfType<EnumMemberDeclarationSyntax>();
    }
}
