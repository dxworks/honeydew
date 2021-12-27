using System.Collections.Generic;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.ReferenceRelations
{
    public interface IReferenceModelVisitor
    {
        public string PrettyPrint();

        public Dictionary<string, int> Visit(ClassModel classModel);
    }
}
