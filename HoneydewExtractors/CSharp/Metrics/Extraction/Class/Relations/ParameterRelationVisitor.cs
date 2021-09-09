using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class ParameterRelationVisitor : IModelVisitor<IClassType>, IRelationVisitor
    {
        public string PrettyPrint()
        {
            return "Parameter Dependency";
        }

        public void Visit(IClassType modelType)
        {
            if (modelType is not IMembersClassType membersClassType)
            {
                return;
            }

            var dependencies = new Dictionary<string, int>();

            foreach (var methodType in membersClassType.Methods)
            {
                foreach (var parameterType in methodType.ParameterTypes)
                {
                    if (dependencies.ContainsKey(parameterType.Type.Name))
                    {
                        dependencies[parameterType.Type.Name]++;
                    }
                    else
                    {
                        dependencies.Add(parameterType.Type.Name, 1);
                    }
                }
            }

            foreach (var constructorType in membersClassType.Constructors)
            {
                foreach (var parameterType in constructorType.ParameterTypes)
                {
                    if (dependencies.ContainsKey(parameterType.Type.Name))
                    {
                        dependencies[parameterType.Type.Name]++;
                    }
                    else
                    {
                        dependencies.Add(parameterType.Type.Name, 1);
                    }
                }
            }

            membersClassType.Metrics.Add(new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dependencies,
                ValueType = dependencies.GetType().ToString()
            });
        }
    }
}
