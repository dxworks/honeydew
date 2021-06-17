using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Models;

namespace HoneydewCore.Processors
{
    public class FullNameDependencyProcessor : IProcessorFunction<SolutionModel, SolutionModel>
    {
        public Func<Processable<SolutionModel>, Processable<SolutionModel>> GetFunction()
        {
            return solutionModelProcessable =>
            {
                var solutionModel = solutionModelProcessable.Value;

                foreach (var classModel in solutionModel.GetEnumerable())
                {
                    var parameterDependenciesMetrics = classModel.Metrics.Where(metric =>
                        metric.ExtractorName == typeof(ParameterDependenciesMetric).FullName);

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
                            var fullClassName =
                                solutionModel.FindClassFullNameInUsings(dependencyDataMetric.Usings,
                                    dependencyName);
                            fullNameDependencies.Add(fullClassName, appearanceCount);
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