using System;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

namespace HoneydewExtractors.Processors
{
    public class HoneydewChooseStrategy : IRelationsMetricChooseStrategy
    {
        public bool Choose(String type)
        {
            return type != nameof(ExternCallsRelationVisitor) && type != nameof(ExternDataRelationVisitor);
        }
    }
}
