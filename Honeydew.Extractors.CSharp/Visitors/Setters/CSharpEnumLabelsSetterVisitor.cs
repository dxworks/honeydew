using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpEnumLabelsSetterVisitor :
    CompositeVisitor<IEnumLabelType>,
    IEnumLabelsSetterVisitor<EnumDeclarationSyntax, SemanticModel, EnumMemberDeclarationSyntax>
{
    public CSharpEnumLabelsSetterVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<IEnumLabelType>> visitors) :
        base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IEnumLabelType CreateWrappedType() => new CSharpEnumLabelType();

    public IEnumerable<EnumMemberDeclarationSyntax> GetWrappedSyntaxNodes(EnumDeclarationSyntax syntaxNode)
    {
        return syntaxNode.Members;
    }
}
