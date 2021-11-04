using System;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

namespace HoneydewExtractors.Processors
{
    public class HoneydewChooseStrategy : IRelationsMetricChooseStrategy
    {
        public bool Choose(Type type)
        {
            return type != typeof(ExternCallsRelationVisitor) && type != typeof(ExternDataRelationVisitor);
        }
    }
}
