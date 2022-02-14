using System;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

namespace HoneydewExtractors.Processors
{
    public class JafaxChooseStrategy : IRelationsMetricChooseStrategy
    {
        public bool Choose(String type)
        {
            return type == nameof(ExternCallsRelationVisitor) ||
                   type == nameof(ExternDataRelationVisitor) ||
                   type == nameof(HierarchyRelationVisitor) ||
                   type == nameof(ReturnValueRelationVisitor) ||
                   type == nameof(DeclarationRelationVisitor);
        }
    }
}
