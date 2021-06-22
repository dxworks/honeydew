using System;
using System.Collections.Generic;
using HoneydewCore.Models.Representations;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    public abstract class DependencyMetric : CSharpMetricExtractor, IRelationMetric
    {
        public DependencyDataMetric DataMetric { get; set; } = new();

        public IList<FileRelation> GetRelations(object metricValue)
        {
            try
            {
                var dataMetric = (DependencyDataMetric) metricValue;

                IList<FileRelation> fileRelations = new List<FileRelation>();

                foreach (var (dependency, count) in dataMetric.Dependencies)
                {
                    var type = Type.GetType(dependency);
                    if (type is {IsPrimitive: true} || IsPrimitive(dependency))
                    {
                        continue;
                    }

                    var relationType = GetType().FullName;
                    var fileRelation = new FileRelation
                    {
                        FileTarget = dependency,
                        RelationCount = count,
                        RelationType = relationType
                    };
                    fileRelations.Add(fileRelation);
                }

                return fileRelations;
            }
            catch (Exception)
            {
                return new List<FileRelation>();
            }
        }

        private static bool IsPrimitive(string dependency)
        {
            return dependency is "object" or "string" or "bool" or "byte" or "char" or "decimal" or "double" or "short"
                or "int" or "long" or "sbyte" or "float" or "ushort" or "uint" or "ulong" or "void";
        }
    }
}