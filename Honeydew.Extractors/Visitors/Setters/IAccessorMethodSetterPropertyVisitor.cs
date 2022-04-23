using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface IAccessorMethodSetterPropertyVisitor<in TSyntaxNode, TSemanticModel, TAccessorMethodSyntaxNode> :
    ISetterVisitor<TSyntaxNode, TSemanticModel, IPropertyType, TAccessorMethodSyntaxNode, IAccessorMethodType>
{
    string ISetterVisitor<TSyntaxNode, TSemanticModel, IPropertyType, TAccessorMethodSyntaxNode, IAccessorMethodType>.
        Name()
    {
        return "Method Accessor";
    }
}
