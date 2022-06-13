using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Extraction;

public interface ILocalVariablesTypeSetterVisitor<in TSyntaxNode, in TSemanticNode, TLocalVariableSyntaxNode,
    TTypeWithLocalVariables> :
    ISetterVisitor<TSyntaxNode, TSemanticNode, TTypeWithLocalVariables, TLocalVariableSyntaxNode, ILocalVariableType>
    where TTypeWithLocalVariables : ITypeWithLocalVariables
{
    string ISetterVisitor<TSyntaxNode, TSemanticNode, TTypeWithLocalVariables, TLocalVariableSyntaxNode,
        ILocalVariableType>.Name()
    {
        return "Local Variable";
    }
}
