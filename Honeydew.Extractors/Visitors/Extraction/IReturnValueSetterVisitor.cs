using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Extraction;

public interface IReturnValueSetterVisitor<in TSyntaxNode, in TSemanticNode, TReturnValueSyntaxNode,
    TTypeWithReturnValue> :
    ISetterVisitor<TSyntaxNode, TSemanticNode, TTypeWithReturnValue, TReturnValueSyntaxNode, IReturnValueType>
    where TTypeWithReturnValue : ITypeWithReturnValue
{
    string ISetterVisitor<TSyntaxNode, TSemanticNode, TTypeWithReturnValue, TReturnValueSyntaxNode, IReturnValueType>.
        Name()
    {
        return "Return Value";
    }

    TTypeWithReturnValue IExtractionVisitor<TSyntaxNode, TSemanticNode, TTypeWithReturnValue>.Visit(
        TSyntaxNode syntaxNode, TSemanticNode semanticModel, TTypeWithReturnValue modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var returnValueType = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.ReturnValue = returnValueType;
        }

        return modelType;
    }
}
