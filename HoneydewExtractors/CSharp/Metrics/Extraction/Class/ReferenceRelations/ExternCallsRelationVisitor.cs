using System.Collections.Generic;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.ReferenceRelations
{
    public class ExternCallsRelationVisitor : IReferenceModelVisitor
    {
        private readonly IAddStrategy _addStrategy;

        public ExternCallsRelationVisitor(IAddStrategy addStrategy)
        {
            _addStrategy = addStrategy;
        }

        public string PrettyPrint()
        {
            return "extCalls";
        }

        public Dictionary<string, int> Visit(ClassModel classModel)
        {
            var dependencies = new Dictionary<string, int>();

            foreach (var methodModel in classModel.Methods)
            {
                foreach (var calledMethod in methodModel.CalledMethods)
                {
                    if (calledMethod.ContainingType != methodModel && calledMethod.Class != null)
                    {
                        _addStrategy.AddDependency(dependencies, calledMethod.Class.Type);
                    }
                }
            }

            foreach (var propertyType in classModel.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var calledMethod in accessor.CalledMethods)
                    {
                        if (calledMethod.ContainingType != accessor && calledMethod.Class != null)
                        {
                            _addStrategy.AddDependency(dependencies, calledMethod.Class.Type);
                        }
                    }
                }
            }

            return dependencies;
        }
    }
}
