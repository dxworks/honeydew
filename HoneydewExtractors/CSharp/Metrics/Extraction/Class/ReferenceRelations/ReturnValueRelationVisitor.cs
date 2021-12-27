using System.Collections.Generic;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.ReferenceRelations
{
    public class ReturnValueRelationVisitor : IReferenceModelVisitor
    {
        private readonly IAddStrategy _addStrategy;

        public ReturnValueRelationVisitor(IAddStrategy addStrategy)
        {
            _addStrategy = addStrategy;
        }

        public string PrettyPrint()
        {
            return "returns";
        }

        public Dictionary<string, int> Visit(ClassModel classModel)
        {
            var dependencies = new Dictionary<string, int>();

            foreach (var methodType in classModel.Methods)
            {
                if (!methodType.IsConstructor && methodType.ReturnValue != null)
                {
                    _addStrategy.AddDependency(dependencies, methodType.ReturnValue.Type);
                }
            }

            return dependencies;
        }
    }
}
