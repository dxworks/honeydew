using System.Collections.Generic;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.ReferenceRelations
{
    public class ExternDataRelationVisitor : IReferenceModelVisitor
    {
        private readonly IAddStrategy _addStrategy;

        public ExternDataRelationVisitor(IAddStrategy addStrategy)
        {
            _addStrategy = addStrategy;
        }

        public string PrettyPrint()
        {
            return "extData";
        }

        public Dictionary<string, int> Visit(ClassModel classModel)
        {
            var dependencies = new Dictionary<string, int>();

            foreach (var methodModel in classModel.Methods)
            {
                foreach (var accessedField in methodModel.AccessedFields)
                {
                    if (accessedField.Field.Class != classModel)
                    {
                        _addStrategy.AddDependency(dependencies, accessedField.Field.Type);
                    }
                }
            }

            foreach (var propertyType in classModel.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var accessedField in accessor.AccessedFields)
                    {
                        if (accessedField.Field.Class != classModel)
                        {
                            _addStrategy.AddDependency(dependencies, accessedField.Field.Type);
                        }
                    }
                }
            }

            return dependencies;
        }
    }
}
