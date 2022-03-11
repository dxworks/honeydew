using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Enum;

public class BasicEnumLabelInfoVisitor : ICSharpEnumLabelVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IEnumLabelType Visit(EnumMemberDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IEnumLabelType modelType)
    {
        modelType.Name = syntaxNode.Identifier.ToString();

        return modelType;
    }
}
