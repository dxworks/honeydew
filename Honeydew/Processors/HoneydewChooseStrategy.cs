using Honeydew.PostExtraction.ReferenceRelations;

namespace Honeydew.Processors;

public class HoneydewChooseStrategy : IRelationsMetricChooseStrategy
{
    public bool Choose(string type)
    {
        return type != ExternCallsRelationVisitor.ExtCallsMetricName && type != ExternDataRelationVisitor.ExtDataMetricName;
    }
}
