using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface IMethodSetterClassVisitor<in TSyntaxNode, TSemanticModel, TMethodSyntaxNode> :
    ISetterVisitor<TSyntaxNode, TSemanticModel, IMembersClassType, TMethodSyntaxNode, IMethodType>
{
    string ISetterVisitor<TSyntaxNode, TSemanticModel, IMembersClassType, TMethodSyntaxNode, IMethodType>.Name()
    {
        return "Method";
    }

    IMembersClassType IExtractionVisitor<TSyntaxNode, TSemanticModel, IMembersClassType>.Visit(TSyntaxNode syntaxNode,
        TSemanticModel semanticModel, IMembersClassType modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var methodType = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.Methods.Add(methodType);
        }

        return modelType;
    }
}
