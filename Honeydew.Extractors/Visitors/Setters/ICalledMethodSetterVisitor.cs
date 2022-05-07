using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface ICalledMethodSetterVisitor<in TSyntaxNode, in TSemanticNode, TCalledMethodSyntaxNode,
    TCallingMethodsType> :
    ISetterVisitor<TSyntaxNode, TSemanticNode, TCallingMethodsType, TCalledMethodSyntaxNode, IMethodCallType>
    where TCallingMethodsType : ICallingMethodsType
{
    string ISetterVisitor<TSyntaxNode, TSemanticNode, TCallingMethodsType, TCalledMethodSyntaxNode, IMethodCallType>.
        Name()
    {
        return "Called Method";
    }

    TCallingMethodsType IExtractionVisitor<TSyntaxNode, TSemanticNode, TCallingMethodsType>.Visit(
        TSyntaxNode syntaxNode, TSemanticNode semanticModel, TCallingMethodsType modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var methodCallType = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.CalledMethods.Add(methodCallType);
        }

        return modelType;
    }
}
