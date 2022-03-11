using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

namespace HoneydewExtractors.Processors
{
    public class JafaxChooseStrategy : IRelationsMetricChooseStrategy
    {
        public bool Choose(string type)
        {
            return type == typeof(ExternCallsRelationVisitor).FullName ||
                   type == typeof(ExternDataRelationVisitor).FullName ||
                   type == typeof(HierarchyRelationVisitor).FullName ||
                   type == typeof(ReturnValueRelationVisitor).FullName ||
                   type == typeof(DeclarationRelationVisitor).FullName;
        }
    }
}
