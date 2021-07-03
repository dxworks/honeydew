using System.Collections.Generic;

namespace HoneydewCore.Models.Representations.ReferenceModel
{
    public interface IClassModelCacheHandler
    {
        public void AddAll(IList<ReferenceClassModel> classModels);
        
        public ReferenceClassModel GetAndAddReference(string type);

        public IList<ReferenceClassModel> GetAllCreatedReferences();

        public IList<ReferenceClassModel> GetAllReferences();
    }
}