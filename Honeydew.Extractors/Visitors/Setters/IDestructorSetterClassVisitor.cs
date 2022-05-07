using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface IDestructorSetterClassVisitor<in TSyntaxNode, TSemanticModel, TDestructorSyntaxNode> :
    ISetterVisitor<TSyntaxNode, TSemanticModel, IMembersClassType, TDestructorSyntaxNode, IDestructorType>
{
    string ISetterVisitor<TSyntaxNode, TSemanticModel, IMembersClassType, TDestructorSyntaxNode, IDestructorType>.Name()
    {
        return "Destructor";
    }

    IMembersClassType IExtractionVisitor<TSyntaxNode, TSemanticModel, IMembersClassType>.Visit(TSyntaxNode syntaxNode,
        TSemanticModel semanticModel, IMembersClassType modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var destructorType = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.Destructor = destructorType;
        }

        return modelType;
    }
}
