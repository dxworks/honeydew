using System;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

namespace HoneydewExtractors.Processors
{
    public class HoneydewChooseStrategy : IRelationsMetricChooseStrategy
    {
        public bool Choose(string type)
        {
            return type != typeof(ExternCallsRelationVisitor).FullName && type != typeof(ExternDataRelationVisitor).FullName;
        }
    }
}
