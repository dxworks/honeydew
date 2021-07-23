using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneydewModels.Representations.ReferenceModel
{
    public record ReferenceSolutionModel
    {
        public IList<ReferenceProjectModel> Projects { get; } = new List<ReferenceProjectModel>();

        private readonly IDictionary<string, ReferenceClassModel> _classModels =
            new Dictionary<string, ReferenceClassModel>();

        private readonly IList<ReferenceClassModel> _createdClassModels = new List<ReferenceClassModel>();

        public ReferenceClassModel FindOrCreateClassModel(string type)
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

        public ReferenceClassModel FindFirstClass(Func<ReferenceClassModel, bool> predicate)
        {
            return (from projectModel in Projects
                from namespaceModel in projectModel.Namespaces
                from classModel in namespaceModel.ClassModels
                select classModel).FirstOrDefault(predicate.Invoke);
        }
    }
}
