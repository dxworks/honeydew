using Honeydew.PostExtraction.ReferenceRelations;

namespace Honeydew.Processors;

public class JafaxChooseStrategy : IRelationsMetricChooseStrategy
{
    public bool Choose(string type)
    {
        return type is ExternCallsRelationVisitor.ExtCallsMetricName or ExternDataRelationVisitor.ExtDataMetricName
            or HierarchyRelationVisitor.HierarchyMetricName or ReturnValueRelationVisitor.ReturnsMetricName
            or DeclarationRelationVisitor.DeclarationsMetricName;
    }
}
