using System.Collections.Generic;

namespace HoneydewCore.Models.Representations.ReferenceModel
{
    public interface IMissingClassModelsHandler
    {
        public ReferenceClassModel GetAndAddReference(string type);

        public IList<ReferenceClassModel> GetAllReferences();
    }
}