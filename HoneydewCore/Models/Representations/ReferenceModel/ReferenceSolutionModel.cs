using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneydewCore.Models.Representations.ReferenceModel
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

        public IList<ReferenceClassModel> GetAllReferences()
        {
            return _classModels.Select(pair => pair.Value).ToList();
        }

        public ReferenceEntity FindFirst(Func<ReferenceEntity, bool> predicate)
        {
            foreach (var projectModel in Projects)
            {
                if (predicate.Invoke(projectModel))
                {
                    return projectModel;
                }

                foreach (var namespaceModel in projectModel.Namespaces)
                {
                    if (predicate.Invoke(namespaceModel))
                    {
                        return namespaceModel;
                    }

                    foreach (var classModel in namespaceModel.ClassModels)
                    {
                        if (predicate.Invoke(classModel))
                        {
                            return classModel;
                        }

                        foreach (var field in classModel.Fields)
                        {
                            if (predicate.Invoke(field))
                            {
                                return field;
                            }
                        }

                        foreach (var methodModel in classModel.Methods)
                        {
                            if (predicate.Invoke(methodModel))
                            {
                                return methodModel;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public ReferenceEntity FindFirstProject(Func<ReferenceProjectModel, bool> predicate)
        {
            return Projects.FirstOrDefault(predicate.Invoke);
        }

        public ReferenceEntity FindFirstNamespace(Func<ReferenceNamespaceModel, bool> predicate)
        {
            return Projects.SelectMany(projectModel => projectModel.Namespaces)
                .FirstOrDefault(predicate.Invoke);
        }

        public ReferenceEntity FindFirstClass(Func<ReferenceClassModel, bool> predicate)
        {
            return (from projectModel in Projects
                from namespaceModel in projectModel.Namespaces
                from classModel in namespaceModel.ClassModels
                select classModel).FirstOrDefault(predicate.Invoke);
        }

        public ReferenceEntity FindFirstField(Func<ReferenceFieldModel, bool> predicate)
        {
            return (from projectModel in Projects
                from namespaceModel in projectModel.Namespaces
                from classModel in namespaceModel.ClassModels
                from field in classModel.Fields
                select field).FirstOrDefault(predicate.Invoke);
        }

        public ReferenceEntity FindFirstMethod(Func<ReferenceMethodModel, bool> predicate)
        {
            return (from projectModel in Projects
                from namespaceModel in projectModel.Namespaces
                from classModel in namespaceModel.ClassModels
                from methodModel in classModel.Methods
                select methodModel).FirstOrDefault(predicate.Invoke);
        }
    }
}