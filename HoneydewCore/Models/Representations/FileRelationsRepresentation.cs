using System.Collections.Generic;
using System.Linq;

namespace HoneydewCore.Models.Representations
{
    public class FileRelationsRepresentation
    {
        public IList<FileRelation> FileRelations { get; set; } = new List<FileRelation>();

        public int TotalRelationsCount(string sourceName)
        {
            return FileRelations
                .Where(fileRelation => fileRelation.FileSource == sourceName)
                .Sum(fileRelation => fileRelation.RelationCount);
        }
    }
}