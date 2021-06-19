using System;
using System.Collections.Generic;
using HoneydewCore.Models.Representations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    public class ParameterDependenciesMetric : CSharpMetricExtractor, ISemanticMetric, ISyntacticMetric, IRelationMetric
    {
        public DependencyDataMetric DataMetric { get; set; } = new();

        public override IMetric GetMetric()
        {
            return new Metric<DependencyDataMetric>(DataMetric);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            DataMetric.Usings.Add(node.Name.ToString());
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            foreach (var parameterSyntax in node.ParameterList.Parameters)
            {
                if (parameterSyntax.Type == null) continue;

                var parameterType = parameterSyntax.Type.ToString();
                if (DataMetric.Dependencies.ContainsKey(parameterType))
                {
                    DataMetric.Dependencies[parameterType]++;
                }
                else
                {
                    DataMetric.Dependencies.Add(parameterType, 1);
                }
            }
        }

        public IList<FileRelation> GetRelations(object metricValue)
        {
            try
            {
                var dataMetric = (DependencyDataMetric) metricValue;

                IList<FileRelation> fileRelations = new List<FileRelation>();

                var relationType = typeof(ParameterDependenciesMetric).FullName;

                foreach (var (dependency, count) in dataMetric.Dependencies)
                {
                    var type = Type.GetType(dependency);
                    if (type is {IsPrimitive: true} || IsPrimitive(dependency))
                    {
                        continue;
                    }

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