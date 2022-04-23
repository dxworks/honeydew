using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface IParameterSetterVisitor<in TSyntaxNode, TSemanticNode, TParameterSyntaxNode, TMethodSignatureType> :
    ISetterVisitor<TSyntaxNode, TSemanticNode, TMethodSignatureType, TParameterSyntaxNode, IParameterType>
    where TMethodSignatureType : IMethodSignatureType
{
    string ISetterVisitor<TSyntaxNode, TSemanticNode, TMethodSignatureType, TParameterSyntaxNode, IParameterType>.Name()
    {
        return "Parameter";
    }

    TMethodSignatureType IExtractionVisitor<TSyntaxNode, TSemanticNode, TMethodSignatureType>.Visit(
        TSyntaxNode syntaxNode, TSemanticNode semanticModel, TMethodSignatureType modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var classModel = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.ParameterTypes.Add(classModel);
        }

        return modelType;
    }
}
