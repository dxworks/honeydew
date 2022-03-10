using Honeydew.PostExtraction.ReferenceRelations;

namespace Honeydew.Processors;

public class JafaxChooseStrategy : IRelationsMetricChooseStrategy
{
    public bool Choose(string type)
    {
        return type is nameof(ExternCallsRelationVisitor) or nameof(ExternDataRelationVisitor)
            or nameof(HierarchyRelationVisitor) or nameof(ReturnValueRelationVisitor)
            or nameof(DeclarationRelationVisitor);
    }
}
