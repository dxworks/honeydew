using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicFieldSetterClassVisitor :
    CompositeVisitor<IFieldType>,
    IFieldSetterClassVisitor<ClassBlockSyntax, SemanticModel, ModifiedIdentifierSyntax>,
    IFieldSetterClassVisitor<StructureBlockSyntax, SemanticModel, ModifiedIdentifierSyntax>
{
    public VisualBasicFieldSetterClassVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<IFieldType>> visitors) :
        base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IFieldType CreateWrappedType() => new VisualBasicFieldModel();

    public IEnumerable<ModifiedIdentifierSyntax> GetWrappedSyntaxNodes(ClassBlockSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes()
            .OfType<FieldDeclarationSyntax>()
            .SelectMany(f => f.Declarators)
            .SelectMany(d => d.Names);
    }

    public IEnumerable<ModifiedIdentifierSyntax> GetWrappedSyntaxNodes(StructureBlockSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes()
            .OfType<FieldDeclarationSyntax>()
            .SelectMany(f => f.Declarators)
            .SelectMany(d => d.Names);
    }
}
