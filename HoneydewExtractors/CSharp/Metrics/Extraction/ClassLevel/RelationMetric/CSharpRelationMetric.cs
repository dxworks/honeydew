using System;
using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public abstract class CSharpRelationMetric : HoneydewCSharpSyntaxWalker,
        IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>, IRelationMetric
    {
        public CSharpSyntacticModel HoneydewSyntacticModel { get; set; }
        public CSharpSemanticModel HoneydewSemanticModel { get; set; }

        public IDictionary<string, int> Dependencies { get; set; } = new Dictionary<string, int>();


        public ExtractionMetricType GetMetricType()
        {
            return ExtractionMetricType.ClassLevel;
        }

        public override IMetricValue GetMetric()
        {
            return new MetricValue<IDictionary<string, int>>(Dependencies);
        }

        public IList<FileRelation> GetRelations(object metricValue)
        {
            try
            {
                var dependencies = (IDictionary<string, int>) metricValue;

                IList<FileRelation> fileRelations = new List<FileRelation>();

                foreach (var (dependency, count) in dependencies)
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
            if (Dependencies.ContainsKey(dependencyName))
            {
                Dependencies[dependencyName]++;
            }
            else
            {
                Dependencies.Add(dependencyName, 1);
            }
        }
    }
}
