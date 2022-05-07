using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface IGenericParameterSetterVisitor<in TSyntaxNode, in TSemanticNode, TGenericParameterSyntaxNode,
    TTypeWithGenericParameters> :
    ISetterVisitor<TSyntaxNode, TSemanticNode, TTypeWithGenericParameters, TGenericParameterSyntaxNode,
        IGenericParameterType>
    where TTypeWithGenericParameters : ITypeWithGenericParameters
{
    string ISetterVisitor<TSyntaxNode, TSemanticNode, TTypeWithGenericParameters, TGenericParameterSyntaxNode,
        IGenericParameterType>.Name()
    {
        return "Generic Parameter";
    }

    TTypeWithGenericParameters IExtractionVisitor<TSyntaxNode, TSemanticNode, TTypeWithGenericParameters>.Visit(
        TSyntaxNode syntaxNode, TSemanticNode semanticModel, TTypeWithGenericParameters modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var genericParameterType = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.GenericParameters.Add(genericParameterType);
        }

        return modelType;
    }
}
