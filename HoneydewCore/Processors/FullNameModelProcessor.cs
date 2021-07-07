using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Models;

namespace HoneydewCore.Processors
{
    public class FullNameModelProcessor : IProcessorFunction<RepositoryModel, RepositoryModel>
    {
        public Func<Processable<RepositoryModel>, Processable<RepositoryModel>> GetFunction()
        {
            return solutionModelProcessable =>
            {
                var repositoryModel = solutionModelProcessable.Value;

                SetFullNameForClassModels(repositoryModel);

                SetFullNameForClassModelComponents(repositoryModel);

                return solutionModelProcessable;
            };
        }

        private static void SetFullNameForClassModelComponents(RepositoryModel repositoryModel)
        {
            foreach (var solutionModel in repositoryModel.Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var (_, namespaceModel) in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            classModel.BaseClassFullName = repositoryModel.FindClassFullName(
                                classModel.BaseClassFullName, namespaceModel, projectModel, solutionModel);

                            for (var i = 0; i < classModel.BaseInterfaces.Count; i++)
                            {
                                classModel.BaseInterfaces[i] =
                                    repositoryModel.FindClassFullName(classModel.BaseInterfaces[i], namespaceModel,
                                        projectModel, solutionModel);
                            }

                            foreach (var methodModel in classModel.Methods)
                            {
                                methodModel.ReturnType = repositoryModel.FindClassFullName(methodModel.ReturnType,
                                    namespaceModel, projectModel, solutionModel);

                                SetContainingClassAndCalledMethodsFullName(methodModel, namespaceModel,
                                    projectModel, solutionModel);
                            }

                            foreach (var methodModel in classModel.Constructors)
                            {
                                SetContainingClassAndCalledMethodsFullName(methodModel, namespaceModel,
                                    projectModel, solutionModel);
                            }

                            ChangeDependencyMetricFullName(repositoryModel, classModel, namespaceModel,
                                projectModel, solutionModel);
                        }
                    }
                }
            }

            void SetContainingClassAndCalledMethodsFullName(MethodModel methodModel, NamespaceModel namespaceModel,
                ProjectModel projectModel, SolutionModel solutionModel)
            {
                methodModel.ContainingClassName = repositoryModel.FindClassFullName(methodModel.ContainingClassName,
                    namespaceModel, projectModel, solutionModel);

                foreach (var methodModelCalledMethod in methodModel.CalledMethods)
                {
                    methodModelCalledMethod.ContainingClassName = repositoryModel.FindClassFullName(
                        methodModelCalledMethod.ContainingClassName, namespaceModel, projectModel, solutionModel);

                    foreach (var parameterModel in methodModelCalledMethod.ParameterTypes)
                    {
                        parameterModel.Type = repositoryModel.FindClassFullName(parameterModel.Type, namespaceModel,
                            projectModel, solutionModel);
                    }
                }
            }
        }

        private static void SetFullNameForClassModels(RepositoryModel repositoryModel)
        {
            foreach (var solutionModel in repositoryModel.Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var (_, namespaceModel) in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            classModel.FullName = repositoryModel.FindClassFullName(classModel.FullName,
                                namespaceModel, projectModel, solutionModel);
                        }
                    }
                }
            }
        }

        private static void ChangeDependencyMetricFullName(RepositoryModel repositoryModel, ClassModel classModel,
            NamespaceModel namespaceModel, ProjectModel projectModel, SolutionModel solutionModel)
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
                    var wasSet = false;

                    var fullClassName =
                        repositoryModel.FindClassFullName(dependencyName, namespaceModel, projectModel, solutionModel,
                            dependencyDataMetric.Usings);
                    if (fullNameDependencies.ContainsKey(fullClassName))
                    {
                        continue;
                    }

                    if (fullClassName != dependencyName)
                    {
                        fullNameDependencies.Add(fullClassName, appearanceCount);
                        wasSet = true;
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
    }
}