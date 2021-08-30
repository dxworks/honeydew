using System;
using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.CSharp.Utils;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public class RelationMetricHolder : IRelationMetricHolder
    {
        private readonly Dictionary<string, Dictionary<IRelationMetric, IDictionary<string, int>>>
            _dependencies = new();

        public void Add(string className, string dependencyName, IRelationMetric relationMetric)
        {
            if (_dependencies.TryGetValue(className, out var relationMetricDictionary))
            {
                if (relationMetricDictionary.TryGetValue(relationMetric, out var dependenciesDictionary))
                {
                    if (dependenciesDictionary.ContainsKey(dependencyName))
                    {
                        dependenciesDictionary[dependencyName]++;
                    }
                    else
                    {
                        dependenciesDictionary.Add(dependencyName, 1);
                    }
                }
                else
                {
                    relationMetricDictionary.Add(relationMetric, new Dictionary<string, int>
                    {
                        { dependencyName, 1 }
                    });
                }
            }
            else
            {
                var dictionary = new Dictionary<IRelationMetric, IDictionary<string, int>>
                {
                    {
                        relationMetric, new Dictionary<string, int>
                        {
                            { dependencyName, 1 }
                        }
                    }
                };
                _dependencies.Add(className, dictionary);
            }
        }

        public IDictionary<string, IDictionary<string, int>> GetDependencies(string className)
        {
            if (!_dependencies.TryGetValue(className, out var dictionary))
            {
                return new Dictionary<string, IDictionary<string, int>>();
            }

            IDictionary<string, IDictionary<string, int>> metricDependencies =
                new Dictionary<string, IDictionary<string, int>>();
            foreach (var (relationMetric, dependencies) in dictionary)
            {
                metricDependencies.Add(relationMetric.PrettyPrint(), dependencies);
            }

            return metricDependencies;
        }

        public IList<FileRelation> GetRelations()
        {
            try
            {
                IList<FileRelation> fileRelations = new List<FileRelation>();

                foreach (var (className, relationDictionary) in _dependencies)
                {
                    foreach (var (relationMetric, dependencyDictionary) in relationDictionary)
                    {
                        foreach (var (dependency, count) in dependencyDictionary)
                        {
                            try
                            {
                                var type = Type.GetType(dependency);
                                if (type is { IsPrimitive: true } || CSharpConstants.IsPrimitive(dependency))
                                {
                                    continue;
                                }
                            }
                            catch (Exception)
                            {
                                // if exception occurs means 'dependency' is not a primitive type
                            }

                            var fileRelation = new FileRelation
                            {
                                Type = relationMetric.PrettyPrint(),
                                FileTarget = dependency,
                                RelationCount = count,
                                FileSource = className
                            };
                            fileRelations.Add(fileRelation);
                        }
                    }
                }

                return fileRelations;
            }
            catch (Exception)
            {
                return new List<FileRelation>();
            }
        }
    }
}
