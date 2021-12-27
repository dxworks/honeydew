using System.Collections.Generic;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.ReferenceRelations
{
    public class LocalVariablesRelationVisitor : IReferenceModelVisitor
    {
        private readonly IAddStrategy _addStrategy;

        public LocalVariablesRelationVisitor(IAddStrategy addStrategy)
        {
            _addStrategy = addStrategy;
        }

        public string PrettyPrint()
        {
            return "Local Variables Dependency";
        }

        public Dictionary<string, int> Visit(ClassModel classModel)
        {
            var dependencies = new Dictionary<string, int>();

            foreach (var propertyType in classModel.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var localVariableType in accessor.LocalVariables)
                    {
                        _addStrategy.AddDependency(dependencies, localVariableType.Type);
                    }

                    foreach (var localFunction in accessor.LocalFunctions)
                    {
                        ExtractLocalVariablesFromLocalFunctions(dependencies, localFunction);
                    }
                }
            }

            foreach (var methodType in classModel.Methods)
            {
                foreach (var localVariableType in methodType.LocalVariables)
                {
                    _addStrategy.AddDependency(dependencies, localVariableType.Type);
                }

                foreach (var localFunction in methodType.LocalFunctions)
                {
                    ExtractLocalVariablesFromLocalFunctions(dependencies, localFunction);
                }
            }

            return dependencies;
        }

        private void ExtractLocalVariablesFromLocalFunctions(IDictionary<string, int> dependencies,
            MethodModel typeWithLocalFunctions)
        {
            foreach (var localFunction in typeWithLocalFunctions.LocalFunctions)
            {
                foreach (var localVariableType in localFunction.LocalVariables)
                {
                    _addStrategy.AddDependency(dependencies, localVariableType.Type);
                }

                foreach (var innerLocalFunction in localFunction.LocalFunctions)
                {
                    ExtractLocalVariablesFromLocalFunctions(dependencies, innerLocalFunction);
                }
            }
        }
    }
}
