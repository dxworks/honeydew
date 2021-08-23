using System.Collections.Generic;

namespace HoneydewCore.ModelRepresentations
{
    public interface IRelationMetric
    {
        string PrettyPrint();
        
        IList<FileRelation> GetRelations(
            IDictionary<string, IDictionary<IRelationMetric, IDictionary<string, int>>> dependencies);
    }
}
