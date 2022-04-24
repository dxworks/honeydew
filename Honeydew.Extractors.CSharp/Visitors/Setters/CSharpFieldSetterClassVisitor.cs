using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpFieldSetterClassVisitor :
    CompositeVisitor<IFieldType>,
    IFieldSetterClassVisitor<TypeDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax>
{
    public CSharpFieldSetterClassVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<IFieldType>> visitors) :
        base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IFieldType CreateWrappedType() => new FieldModel();

    public IEnumerable<VariableDeclaratorSyntax> GetWrappedSyntaxNodes(TypeDeclarationSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<BaseFieldDeclarationSyntax>().SelectMany(
            fieldDeclaration => fieldDeclaration.Declaration.Variables);
    }
}
