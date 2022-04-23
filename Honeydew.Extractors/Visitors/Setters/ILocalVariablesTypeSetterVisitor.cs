using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface ILocalVariablesTypeSetterVisitor<in TSyntaxNode, TSemanticNode, TLocalVariableSyntaxNode,
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
