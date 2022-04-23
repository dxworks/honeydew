using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface IAttributeSetterVisitor<in TSyntaxNode, TSemanticNode, TAttributeSyntaxNode, TTypeWithAttributes> :
    ISetterVisitor<TSyntaxNode, TSemanticNode, TTypeWithAttributes, TAttributeSyntaxNode, IAttributeType>
    where TTypeWithAttributes : ITypeWithAttributes
{
    string ISetterVisitor<TSyntaxNode, TSemanticNode, TTypeWithAttributes, TAttributeSyntaxNode,
        IAttributeType>.Name()
    {
        return "Attribute";
    }

    TTypeWithAttributes IExtractionVisitor<TSyntaxNode, TSemanticNode, TTypeWithAttributes>.Visit(
        TSyntaxNode syntaxNode, TSemanticNode semanticModel, TTypeWithAttributes modelType)
    {
        foreach (var attributeWrapper in GetWrappedSyntaxNodes(syntaxNode))
        {
            var attributeType = ApplyContainedVisitors(attributeWrapper, CreateWrappedType(), semanticModel);

            modelType.Attributes.Add(attributeType);
        }

        return modelType;
    }
}
