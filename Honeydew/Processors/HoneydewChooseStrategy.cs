using Honeydew.PostExtraction.ReferenceRelations;

namespace Honeydew.Processors;

public class HoneydewChooseStrategy : IRelationsMetricChooseStrategy
{
    public bool Choose(string type)
    {
        return type != nameof(ExternCallsRelationVisitor) && type != nameof(ExternDataRelationVisitor);
    }
}
