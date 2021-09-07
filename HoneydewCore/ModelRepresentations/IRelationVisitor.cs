using System.Collections.Generic;

namespace HoneydewCore.ModelRepresentations
{
    public interface IRelationVisitor
    {
        string PrettyPrint();
        
        IList<FileRelation> GetRelations(
            IDictionary<string, IDictionary<IRelationVisitor, IDictionary<string, int>>> dependencies);
    }
}
