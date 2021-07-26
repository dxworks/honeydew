using System.Collections.Generic;
using HoneydewModels.CSharp;

namespace HoneydewCore.ModelRepresentations
{
    public interface IMetricRelationsProvider
    {
        IList<FileRelation> GetFileRelations(ClassMetric classMetric);
    }
}
