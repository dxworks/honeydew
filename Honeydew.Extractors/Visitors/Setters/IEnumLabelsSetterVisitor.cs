using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface IEnumLabelsSetterVisitor<in TSyntaxNode, TSemanticModel, TEnumLabelSyntaxNode> :
    ISetterVisitor<TSyntaxNode, TSemanticModel, IEnumType, TEnumLabelSyntaxNode, IEnumLabelType>
{
    string ISetterVisitor<TSyntaxNode, TSemanticModel, IEnumType, TEnumLabelSyntaxNode, IEnumLabelType>.Name()
    {
        return "Enum Label";
    }

    IEnumType IExtractionVisitor<TSyntaxNode, TSemanticModel, IEnumType>.Visit(TSyntaxNode syntaxNode,
        TSemanticModel semanticModel, IEnumType modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var enumLabelType = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.Labels.Add(enumLabelType);
        }

        return modelType;
    }
}
