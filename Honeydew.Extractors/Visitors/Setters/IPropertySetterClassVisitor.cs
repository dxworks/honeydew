using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface IPropertySetterClassVisitor<in TSyntaxNode, TSemanticModel, TPropertySyntaxNode> :
    ISetterVisitor<TSyntaxNode, TSemanticModel, IMembersClassType, TPropertySyntaxNode, IPropertyType>
{
    string ISetterVisitor<TSyntaxNode, TSemanticModel, IMembersClassType, TPropertySyntaxNode, IPropertyType>.Name()
    {
        return "Property";
    }

    IMembersClassType IExtractionVisitor<TSyntaxNode, TSemanticModel, IMembersClassType>.Visit(TSyntaxNode syntaxNode,
        TSemanticModel semanticModel, IMembersClassType modelType)
    {
        if (modelType is not IPropertyMembersClassType propertyMembersClassType)
        {
            return modelType;
        }

        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var propertyModel = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            propertyMembersClassType.Properties.Add(propertyModel);
        }

        return propertyMembersClassType;
    }
}
