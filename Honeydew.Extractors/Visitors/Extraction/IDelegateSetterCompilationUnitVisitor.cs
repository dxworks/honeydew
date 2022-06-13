using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Extraction;

public interface IDelegateSetterCompilationUnitVisitor<in TSyntaxNode, in TSemanticModel, TDelegateSyntaxNode> :
    ISetterVisitor<TSyntaxNode, TSemanticModel, ICompilationUnitType, TDelegateSyntaxNode, IDelegateType>

{
    string ISetterVisitor<TSyntaxNode, TSemanticModel, ICompilationUnitType, TDelegateSyntaxNode, IDelegateType>.Name()
    {
        return "Delegate";
    }

    ICompilationUnitType IExtractionVisitor<TSyntaxNode, TSemanticModel, ICompilationUnitType>.Visit(
        TSyntaxNode syntaxNode, TSemanticModel semanticModel, ICompilationUnitType modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var delegateType = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.ClassTypes.Add(delegateType);
        }

        return modelType;
    }
}
