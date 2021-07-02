using System.Collections.Generic;
using System.Linq;

namespace HoneydewCore.Models.Representations.ReferenceModel
{
    public class MissingClassModelsHandler : IMissingClassModelsHandler
    {
        private readonly IDictionary<string, ReferenceClassModel> _classModels =
            new Dictionary<string, ReferenceClassModel>();

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

            return classModel;
        }

        public IList<ReferenceClassModel> GetAllReferences()
        {
            return _classModels.Select(pair => pair.Value).ToList();
        }
    }
}