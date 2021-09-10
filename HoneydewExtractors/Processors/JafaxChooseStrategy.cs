using System;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

namespace HoneydewExtractors.Processors
{
    public class JafaxChooseStrategy : IRelationsMetricChooseStrategy
    {
        public bool Choose(Type type)
        {
            return type == typeof(ExternCallsRelationVisitor);
        }
    }
}
