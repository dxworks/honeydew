using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface IFieldSetterClassVisitor<in TSyntaxNode, in TSemanticModel, TFieldSyntaxNode> :
    ISetterVisitor<TSyntaxNode, TSemanticModel, IMembersClassType, TFieldSyntaxNode, IFieldType?>
{
    string ISetterVisitor<TSyntaxNode, TSemanticModel, IMembersClassType, TFieldSyntaxNode, IFieldType?>.Name()
    {
        return "Field";
    }

    IMembersClassType IExtractionVisitor<TSyntaxNode, TSemanticModel, IMembersClassType>.Visit(TSyntaxNode syntaxNode,
        TSemanticModel semanticModel, IMembersClassType modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var fieldType = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            if (fieldType is not null && !string.IsNullOrEmpty(fieldType.Name))
            {
                modelType.Fields.Add(fieldType);
            }
        }

        return modelType;
    }
}
