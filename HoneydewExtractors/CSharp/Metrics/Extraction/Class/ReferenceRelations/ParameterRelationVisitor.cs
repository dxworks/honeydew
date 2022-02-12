using System.Collections.Generic;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.ReferenceRelations
{
    public class ParameterRelationVisitor : IReferenceModelVisitor
    {
        private readonly IAddStrategy _addStrategy;

        public ParameterRelationVisitor(IAddStrategy addStrategy)
        {
            _addStrategy = addStrategy;
        }

        public string PrettyPrint()
        {
            return "Parameter Dependency";
        }

        public Dictionary<string, int> Visit(ClassModel classModel)
        {
            var dependencies = new Dictionary<string, int>();

            foreach (var methodType in classModel.Methods)
            {
                foreach (var parameterType in methodType.Parameters)
                {
                    _addStrategy.AddDependency(dependencies, parameterType.Type);
                }
            }

            return dependencies;
        }
    }
}
