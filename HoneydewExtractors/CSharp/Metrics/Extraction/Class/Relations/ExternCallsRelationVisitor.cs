using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class ExternCallsRelationVisitor : IModelVisitor<IClassType>, IRelationVisitor
    {
        public string PrettyPrint()
        {
            return "extCalls";
        }

        public void Visit(IClassType modelType)
        {
            if (modelType is not IPropertyMembersClassType classTypeWithProperties)
            {
                return;
            }

            var dependencies = new Dictionary<string, int>();

            foreach (var methodType in classTypeWithProperties.Methods)
            {
                foreach (var calledMethod in methodType.CalledMethods)
                {
                    if (calledMethod.ContainingTypeName != modelType.Name)
                    {
                        AddDependency(calledMethod.ContainingTypeName);
                    }
                }
            }

            foreach (var constructorType in classTypeWithProperties.Constructors)
            {
                foreach (var calledMethod in constructorType.CalledMethods)
                {
                    if (calledMethod.ContainingTypeName != modelType.Name)
                    {
                        AddDependency(calledMethod.ContainingTypeName);
                    }
                }
            }

            foreach (var propertyType in classTypeWithProperties.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var calledMethod in accessor.CalledMethods)
                    {
                        if (calledMethod.ContainingTypeName != modelType.Name)
                        {
                            AddDependency(calledMethod.ContainingTypeName);
                        }
                    }
                }
            }

            classTypeWithProperties.Metrics.Add(new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dependencies,
                ValueType = dependencies.GetType().ToString()
            });

            void AddDependency(string dependencyName)
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
        }
    }
}
