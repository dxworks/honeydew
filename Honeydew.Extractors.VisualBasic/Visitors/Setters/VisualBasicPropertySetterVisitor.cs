using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicPropertySetterVisitor :
    CompositeVisitor<IPropertyType>,
    IPropertySetterClassVisitor<ClassBlockSyntax, SemanticModel, PropertyStatementSyntax>,
    IPropertySetterClassVisitor<InterfaceBlockSyntax, SemanticModel, PropertyStatementSyntax>,
    IPropertySetterClassVisitor<StructureBlockSyntax, SemanticModel, PropertyStatementSyntax>
{
    public VisualBasicPropertySetterVisitor(ILogger logger, IEnumerable<ITypeVisitor<IPropertyType>> visitors) :
        base(
            logger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IPropertyType CreateWrappedType() => new VisualBasicPropertyModel();

    public IEnumerable<PropertyStatementSyntax> GetWrappedSyntaxNodes(ClassBlockSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<PropertyStatementSyntax>();
    }

    public IEnumerable<PropertyStatementSyntax> GetWrappedSyntaxNodes(InterfaceBlockSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<PropertyStatementSyntax>();
    }

    public IEnumerable<PropertyStatementSyntax> GetWrappedSyntaxNodes(StructureBlockSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<PropertyStatementSyntax>();
    }
}
