using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class DeclarationRelationVisitor : IModelVisitor<IClassType>, IRelationVisitor
    {
        private readonly LocalVariablesRelationVisitor _localVariablesRelationVisitor;
        private readonly ParameterRelationVisitor _parameterRelationVisitor;
        private readonly FieldsRelationVisitor _fieldsRelationVisitor;
        private readonly PropertiesRelationVisitor _propertiesRelationVisitor;

        public DeclarationRelationVisitor()
        {
        }

        public DeclarationRelationVisitor(LocalVariablesRelationVisitor localVariablesRelationVisitor,
            ParameterRelationVisitor parameterRelationVisitor, FieldsRelationVisitor fieldsRelationVisitor,
            PropertiesRelationVisitor propertiesRelationVisitor)
        {
            _localVariablesRelationVisitor = localVariablesRelationVisitor;
            _parameterRelationVisitor = parameterRelationVisitor;
            _fieldsRelationVisitor = fieldsRelationVisitor;
            _propertiesRelationVisitor = propertiesRelationVisitor;
        }

        public string PrettyPrint()
        {
            return "declarations";
        }

        public void Visit(IClassType modelType)
        {
            if (modelType is not IMembersClassType membersClassType)
            {
                return;
            }

            var dependencies = new Dictionary<string, int>();

            if (_parameterRelationVisitor != null)
            {
                AddToDictionary(dependencies, _parameterRelationVisitor.GetDependencies(membersClassType));
            }

            if (_fieldsRelationVisitor != null)
            {
                AddToDictionary(dependencies, _fieldsRelationVisitor.GetDependencies(membersClassType));
            }

            if (modelType is IPropertyMembersClassType classTypeWithProperties)
            {
                if (_localVariablesRelationVisitor != null)
                {
                    AddToDictionary(dependencies,
                        _localVariablesRelationVisitor.GetDependencies(classTypeWithProperties));
                }

                if (_propertiesRelationVisitor != null)
                {
                    AddToDictionary(dependencies,
                        _propertiesRelationVisitor.GetDependencies(classTypeWithProperties));
                }
            }

            modelType.Metrics.Add(new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dependencies,
                ValueType = dependencies.GetType().ToString()
            });
        }

        private void AddToDictionary(IDictionary<string, int> destination, Dictionary<string, int> source)
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
