using System.Collections.Generic;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.ReferenceRelations
{
    public class DeclarationRelationVisitor : IReferenceModelVisitor
    {
        private readonly LocalVariablesRelationVisitor _localVariablesRelationVisitor;
        private readonly ParameterRelationVisitor _parameterRelationVisitor;
        private readonly FieldsAndPropertiesRelationVisitor _fieldsAndPropertiesRelationVisitor;

        public DeclarationRelationVisitor(LocalVariablesRelationVisitor localVariablesRelationVisitor,
            ParameterRelationVisitor parameterRelationVisitor,
            FieldsAndPropertiesRelationVisitor fieldsAndPropertiesRelationVisitor)
        {
            _localVariablesRelationVisitor = localVariablesRelationVisitor;
            _parameterRelationVisitor = parameterRelationVisitor;
            _fieldsAndPropertiesRelationVisitor = fieldsAndPropertiesRelationVisitor;
        }

        public string PrettyPrint()
        {
            return "declarations";
        }

        public Dictionary<string, int> Visit(ClassModel classModel)
        {
            var dependencies = new Dictionary<string, int>();

            if (_parameterRelationVisitor != null)
            {
                AddToDictionary(dependencies, _parameterRelationVisitor.Visit(classModel));
            }

            if (_fieldsAndPropertiesRelationVisitor != null)
            {
                AddToDictionary(dependencies, _fieldsAndPropertiesRelationVisitor.Visit(classModel));
            }

            if (_localVariablesRelationVisitor != null)
            {
                AddToDictionary(dependencies, _localVariablesRelationVisitor.Visit(classModel));
            }

            return dependencies;
        }

        private static void AddToDictionary(IDictionary<string, int> destination, Dictionary<string, int> source)
        {
            foreach (var (key, count) in source)
            {
                if (destination.ContainsKey(key))
                {
                    destination[key] += count;
                }
                else
                {
                    destination.Add(key, count);
                }
            }
        }
    }
}
