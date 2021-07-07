using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Models;

namespace HoneydewCore.Processors
{
    public class FullNameDependencyProcessor : IProcessorFunction<RepositoryModel, RepositoryModel>
    {
        public Func<Processable<RepositoryModel>, Processable<RepositoryModel>> GetFunction()
        {
            return solutionModelProcessable =>
            {
                var repositoryModel = solutionModelProcessable.Value;

                foreach (var classModel in repositoryModel.GetEnumerable())
                {
                    var parameterDependenciesMetrics = classModel.Metrics.Where(metric =>
                        typeof(DependencyMetric).IsAssignableFrom(Type.GetType(metric.ExtractorName)));

                    foreach (var metric in parameterDependenciesMetrics)
                    {
                        var dependencyDataMetric = metric.Value as DependencyDataMetric;
                        if (dependencyDataMetric == null)
                        {
                            continue;
                        }

                        IDictionary<string, int> fullNameDependencies = new Dictionary<string, int>();
                        foreach (var (dependencyName, appearanceCount) in dependencyDataMetric.Dependencies)
                        {
                            bool wasSet = false;
                            foreach (var solutionModel in repositoryModel.Solutions)
                            {
                                var fullClassName =
                                    solutionModel.FindClassFullNameInUsings(dependencyDataMetric.Usings,
                                        dependencyName, classModel.Namespace);
                                if (fullClassName != dependencyName)
                                {
                                    fullNameDependencies.Add(fullClassName, appearanceCount);
                                    wasSet = true;
                                }
                            }

                            if (!wasSet)
                            {
                                fullNameDependencies.Add(dependencyName, appearanceCount);
                            }
                        }

                        dependencyDataMetric.Dependencies = fullNameDependencies;
                        metric.Value = dependencyDataMetric;
                    }
                }

                return solutionModelProcessable;
            };
        }
    }
}