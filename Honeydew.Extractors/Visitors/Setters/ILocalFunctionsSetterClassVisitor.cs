using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface ILocalFunctionsSetterClassVisitor<in TSyntaxNode, in TSemanticNode, TLocalFunctionSyntaxNode,
    TTypeWithLocalFunctions> :
    ISetterVisitor<TSyntaxNode, TSemanticNode, TTypeWithLocalFunctions, TLocalFunctionSyntaxNode,
        IMethodTypeWithLocalFunctions>
    where TTypeWithLocalFunctions : ITypeWithLocalFunctions
{
    string ISetterVisitor<TSyntaxNode, TSemanticNode, TTypeWithLocalFunctions, TLocalFunctionSyntaxNode,
        IMethodTypeWithLocalFunctions>.Name()
    {
        return "Local Function";
    }

    TTypeWithLocalFunctions IExtractionVisitor<TSyntaxNode, TSemanticNode, TTypeWithLocalFunctions>.Visit(
        TSyntaxNode syntaxNode, TSemanticNode semanticModel, TTypeWithLocalFunctions modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var localFunctionType = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.LocalFunctions.Add(localFunctionType);
        }

        return modelType;
    }
}
