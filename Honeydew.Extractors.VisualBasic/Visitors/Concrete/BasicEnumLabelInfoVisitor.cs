using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class BasicEnumLabelInfoVisitor : IExtractionVisitor<EnumMemberDeclarationSyntax, SemanticModel, IEnumLabelType>
{
    public IEnumLabelType Visit(EnumMemberDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IEnumLabelType modelType)
    {
        modelType.Name = syntaxNode.Identifier.ToString();

        return modelType;
    }
}
