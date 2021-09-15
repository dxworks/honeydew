using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneydewModels.CSharp.ReferenceModel
{
    public record ReferenceSolutionModel
    {
        public IList<ReferenceProjectModel> Projects { get; } = new List<ReferenceProjectModel>();

        public IList<ReferenceClassModel> CreatedClassModels { get; } = new List<ReferenceClassModel>();
        
        private readonly IDictionary<string, ReferenceClassModel> _classModels =
            new Dictionary<string, ReferenceClassModel>();        

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
            CreatedClassModels.Add(classModel);

            return classModel;
        }

        public ReferenceClassModel FindFirstClass(Func<ReferenceClassModel, bool> predicate)
        {
            return (from projectModel in Projects
                from compilationUnit in projectModel.CompilationUnits
                from classModel in compilationUnit.ClassModels
                select classModel).FirstOrDefault(predicate.Invoke);
        }
    }
}
