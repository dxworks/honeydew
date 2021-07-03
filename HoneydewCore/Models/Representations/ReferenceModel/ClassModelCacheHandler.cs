using System.Collections.Generic;
using System.Linq;

namespace HoneydewCore.Models.Representations.ReferenceModel
{
    internal class ClassModelCacheHandler : IClassModelCacheHandler
    {
        private readonly IDictionary<string, ReferenceClassModel> _classModels =
            new Dictionary<string, ReferenceClassModel>();

        private readonly IList<ReferenceClassModel> _createdClassModels = new List<ReferenceClassModel>();

        public void AddAll(IList<ReferenceClassModel> classModels)
        {
            foreach (var classModel in classModels)
            {
                _classModels.Add(classModel.Name, classModel);
            }
        }

        public ReferenceClassModel GetAndAddReference(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return null;
            }

            if (_classModels.TryGetValue(type, out var referenceClassModel))
            {
                return referenceClassModel;
            }

            ReferenceClassModel classModel = new()
            {
                Name = type,
            };

            _classModels.Add(type, classModel);
            _createdClassModels.Add(classModel);

            return classModel;
        }

        public IList<ReferenceClassModel> GetAllCreatedReferences()
        {
            return _createdClassModels;
        }

        public IList<ReferenceClassModel> GetAllReferences()
        {
            return _classModels.Select(pair => pair.Value).ToList();
        }
    }
}