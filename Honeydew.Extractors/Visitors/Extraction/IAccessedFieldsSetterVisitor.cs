using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Extraction;

public interface IAccessedFieldsSetterVisitor<in TSyntaxNode, in TSemanticNode, TAccessedFieldSyntaxNode,
    TTypeWithAccessedFields> :
    ISetterVisitor<TSyntaxNode, TSemanticNode, TTypeWithAccessedFields, TAccessedFieldSyntaxNode, AccessedField?>
    where TTypeWithAccessedFields : IContainedTypeWithAccessedFields
{
    string ISetterVisitor<TSyntaxNode, TSemanticNode, TTypeWithAccessedFields, TAccessedFieldSyntaxNode, AccessedField?>
        .Name()
    {
        return "Accessed Field";
    }

    TTypeWithAccessedFields
        IExtractionVisitor<TSyntaxNode, TSemanticNode, TTypeWithAccessedFields>.Visit(
            TSyntaxNode syntaxNode, TSemanticNode semanticModel, TTypeWithAccessedFields modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var accessedField = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            if (accessedField is not null && !string.IsNullOrEmpty(accessedField.Name))
            {
                modelType.AccessedFields.Add(accessedField);
            }
        }

        return modelType;
    }
}
