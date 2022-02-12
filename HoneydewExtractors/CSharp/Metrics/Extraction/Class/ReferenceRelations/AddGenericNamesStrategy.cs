using System.Collections.Generic;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.ReferenceRelations
{
    public class AddGenericNamesStrategy : IAddStrategy
    {
        public void AddDependency(IDictionary<string, int> dependencies, EntityType type)
        {
            AddDependency(dependencies, type.FullType);
        }

        private static void AddDependency(IDictionary<string, int> dependencies, string dependencyName)
        {
            if (dependencies.ContainsKey(dependencyName))
            {
                dependencies[dependencyName]++;
            }
            else
            {
                dependencies.Add(dependencyName, 1);
            }
        }

        private static void AddDependency(IDictionary<string, int> dependencies, GenericType type)
        {
            switch (type.Reference)
            {
                case ClassModel classModel:
                    AddDependency(dependencies, classModel.Name);
                    break;
                case DelegateModel delegateModel:
                    AddDependency(dependencies, delegateModel.Name);
                    break;
            }

            foreach (var containedType in type.ContainedTypes)
            {
                AddDependency(dependencies, containedType);
            }
        }
    }
}
