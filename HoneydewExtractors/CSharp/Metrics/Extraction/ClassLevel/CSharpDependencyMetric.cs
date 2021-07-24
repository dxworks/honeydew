using System;
using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel
{
    // ICompilationUnitMetric is used because The Metric uses the Usings statements
    public abstract class CSharpDependencyMetric : HoneydewCSharpSyntaxWalker,
        IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>, IRelationMetric
    {
        public CSharpSyntacticModel HoneydewSyntacticModel { get; set; }
        public CSharpSemanticModel HoneydewSemanticModel { get; set; }
        public CSharpDependencyDataMetric DataMetric { get; set; } = new();

        // todo change to class level
        public ExtractionMetricType GetMetricType()
        {
            return ExtractionMetricType.CompilationUnitLevel;
        }

        public override IMetricValue GetMetric()
        {
            return new MetricValue<CSharpDependencyDataMetric>(DataMetric);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            DataMetric.Usings.Add(node.Name.ToString());
        }

        public IList<FileRelation> GetRelations(object metricValue)
        {
            try
            {
                var dataMetric = (CSharpDependencyDataMetric) metricValue;
        
                IList<FileRelation> fileRelations = new List<FileRelation>();
        
                foreach (var (dependency, count) in dataMetric.Dependencies)
                {
                    var type = Type.GetType(dependency);
                    if (type is {IsPrimitive: true} || CSharpConstants.IsPrimitive(dependency))
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

        protected void AddDependency(string dependencyName)
        {
            if (DataMetric.Dependencies.ContainsKey(dependencyName))
            {
                DataMetric.Dependencies[dependencyName]++;
            }
            else
            {
                DataMetric.Dependencies.Add(dependencyName, 1);
            }
        }
    }
}
