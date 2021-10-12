using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.CSharp.Utils;
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

            var dependencies = GetDependencies(membersClassType);

            membersClassType.Metrics.Add(new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dependencies,
                ValueType = dependencies.GetType().ToString()
            });
        }

        public Dictionary<string, int> GetDependencies(IMembersClassType membersClassType)
        {
            var dependencies = new Dictionary<string, int>();

            foreach (var methodType in membersClassType.Methods)
            {
                foreach (var parameterType in methodType.ParameterTypes)
                {
                    var typeName = CSharpConstants.GetNonNullableName(parameterType.Type.Name);
                    if (dependencies.ContainsKey(typeName))
                    {
                        dependencies[typeName]++;
                    }
                    else
                    {
                        dependencies.Add(typeName, 1);
                    }
                }
            }

            foreach (var constructorType in membersClassType.Constructors)
            {
                foreach (var parameterType in constructorType.ParameterTypes)
                {
                    var typeName = CSharpConstants.GetNonNullableName(parameterType.Type.Name);
                    if (dependencies.ContainsKey(typeName))
                    {
                        dependencies[typeName]++;
                    }
                    else
                    {
                        dependencies.Add(typeName, 1);
                    }
                }
            }

            return dependencies;
        }
    }
}
