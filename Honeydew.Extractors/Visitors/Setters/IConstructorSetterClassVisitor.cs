using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface IConstructorSetterClassVisitor<in TSyntaxNode, TSemanticModel, TConstructorSyntaxNode> :
    ISetterVisitor<TSyntaxNode, TSemanticModel, IMembersClassType, TConstructorSyntaxNode, IConstructorType>

{
    string ISetterVisitor<TSyntaxNode, TSemanticModel, IMembersClassType, TConstructorSyntaxNode, IConstructorType>.
        Name()
    {
        return "Constructor";
    }

    IMembersClassType IExtractionVisitor<TSyntaxNode, TSemanticModel, IMembersClassType>.Visit(TSyntaxNode syntaxNode,
        TSemanticModel semanticModel, IMembersClassType modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var constructorType = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.Constructors.Add(constructorType);
        }

        return modelType;
    }
}
